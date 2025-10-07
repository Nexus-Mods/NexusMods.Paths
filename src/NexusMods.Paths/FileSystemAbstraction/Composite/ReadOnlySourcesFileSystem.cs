using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexusMods.Paths.FileProviders;
using NexusMods.Paths.Utilities;

namespace NexusMods.Paths;

/// <summary>
/// A file system that reads through to an upstream <see cref=\"IFileSystem\"/> and falls back to
/// one or more read-only sources mounted at given mount points. On the first write to a path that
/// only exists in a read-only source, the file is copied into the upstream file system (copy-on-write)
/// and subsequent operations use the upstream file.
/// </summary>
public sealed class ReadOnlySourcesFileSystem : BaseFileSystem, IReadOnlyFileSystem
{
    private readonly IFileSystem _upstream;
    private readonly IReadOnlyList<IReadOnlyFileSource> _sources;

    public ReadOnlySourcesFileSystem(IFileSystem upstream, IEnumerable<IReadOnlyFileSource> sources)
    {
        _upstream = upstream;
        _sources = sources.ToArray();
    }

    public override IFileSystem CreateOverlayFileSystem(
        Dictionary<AbsolutePath, AbsolutePath> pathMappings,
        Dictionary<KnownPath, AbsolutePath> knownPathMappings,
        bool convertCrossPlatformPaths = false)
    {
        var overlayUpstream = _upstream.CreateOverlayFileSystem(pathMappings, knownPathMappings, convertCrossPlatformPaths);
        return new ReadOnlySourcesFileSystem(overlayUpstream, _sources);
    }

    public override int ReadBytesRandomAccess(AbsolutePath path, Span<byte> bytes, long offset)
    {
        if (_upstream.FileExists(path))
            return _upstream.ReadBytesRandomAccess(path, bytes, offset);

        if (TryResolveSource(path, out var src, out var rel))
        {
            using var chunk = src.GetFileData(rel, (ulong)offset, (ulong)bytes.Length);
            var toCopy = (int)Math.Min((ulong)bytes.Length, chunk.DataLength);
            if (toCopy <= 0) return 0;
            chunk.Data.Span.Slice(0, toCopy).CopyTo(bytes);
            return toCopy;
        }

        throw new FileNotFoundException($"File not found: {path}");
    }

    public override async Task<int> ReadBytesRandomAccessAsync(AbsolutePath path, Memory<byte> bytes, long offset, CancellationToken cancellationToken = default)
    {
        if (_upstream.FileExists(path))
            return await _upstream.ReadBytesRandomAccessAsync(path, bytes, offset, cancellationToken);

        if (TryResolveSource(path, out var src, out var rel))
        {
            await using var s = src.OpenRead(rel);
            if (offset > 0) s.Seek(offset, SeekOrigin.Begin);
            return await s.ReadAtLeastAsync(bytes, bytes.Length, false, cancellationToken);
        }

        throw new FileNotFoundException($"File not found: {path}");
    }

    protected override IFileEntry InternalGetFileEntry(AbsolutePath path)
    {
        if (_upstream.FileExists(path))
            return _upstream.GetFileEntry(path);

        if (TryResolveSource(path, out var src, out var rel))
        {
            using var s = src.OpenRead(rel);
            var len = (ulong)s.Length;
            return new VirtualReadOnlyFileEntry(this, path, len);
        }

        // Fallback to upstream behavior (will typically create placeholder or throw depending on impl)
        return _upstream.GetFileEntry(path);
    }

    protected override IDirectoryEntry InternalGetDirectoryEntry(AbsolutePath path)
    {
        if (_upstream.DirectoryExists(path))
            return _upstream.GetDirectoryEntry(path);

        // If any source has a file under this path, expose a virtual directory
        if (SourceHasAnyUnder(path))
            return new VirtualDirectoryEntry();

        return _upstream.GetDirectoryEntry(path);
    }

    protected override IEnumerable<AbsolutePath> InternalEnumerateFiles(AbsolutePath directory, string pattern, bool recursive)
    {
        var set = new HashSet<AbsolutePath>();
        foreach (var f in _upstream.EnumerateFiles(directory, pattern, recursive))
        {
            if (set.Add(f)) yield return f;
        }

        foreach (var p in EnumerateSourceFilesUnder(directory, recursive))
        {
            if (!EnumeratorHelpers.MatchesPattern(pattern, p.GetFullPath(), MatchType.Win32))
                continue;
            if (set.Add(p)) yield return p;
        }
    }

    protected override IEnumerable<AbsolutePath> InternalEnumerateDirectories(AbsolutePath directory, string pattern, bool recursive)
    {
        var set = new HashSet<AbsolutePath>();
        foreach (var d in _upstream.EnumerateDirectories(directory, pattern, recursive))
        {
            if (set.Add(d)) yield return d;
        }

        foreach (var d in EnumerateSourceDirectoriesUnder(directory, recursive))
        {
            if (!EnumeratorHelpers.MatchesPattern(pattern, d.GetFullPath(), MatchType.Win32))
                continue;
            if (set.Add(d)) yield return d;
        }
    }

    protected override IEnumerable<IFileEntry> InternalEnumerateFileEntries(AbsolutePath directory, string pattern, bool recursive)
    {
        var yielded = new HashSet<AbsolutePath>();
        foreach (var entry in _upstream.EnumerateFileEntries(directory, pattern, recursive))
        {
            if (yielded.Add(entry.Path)) yield return entry;
        }

        foreach (var p in EnumerateSourceFilesUnder(directory, recursive))
        {
            if (!EnumeratorHelpers.MatchesPattern(pattern, p.GetFullPath(), MatchType.Win32))
                continue;
            if (yielded.Contains(p)) continue;
            if (TryResolveSource(p, out var src, out var rel))
            {
                using var s = src.OpenRead(rel);
                yield return new VirtualReadOnlyFileEntry(this, p, (ulong)s.Length);
            }
        }
    }

    protected override Stream InternalOpenFile(AbsolutePath path, FileMode mode, FileAccess access, FileShare share)
    {
        // If upstream already has it, just delegate
        if (_upstream.FileExists(path) || !TryResolveSource(path, out var src, out var rel))
        {
            return _upstream.OpenFile(path, mode, access, share);
        }

        // Path maps to a read-only source
        if (access == FileAccess.Read)
        {
            return src.OpenRead(rel);
        }

        // Copy-on-write for any write access
        EnsureCopiedToUpstream(path, src, rel);
        // After copy, open in upstream. If the requested mode would fail because we created the file,
        // prefer opening it.
        var effectiveMode = mode == FileMode.CreateNew ? FileMode.Open : mode;
        return _upstream.OpenFile(path, effectiveMode, access, share);
    }

    protected override void InternalCreateDirectory(AbsolutePath path)
    {
        _upstream.CreateDirectory(path);
    }

    protected override bool InternalDirectoryExists(AbsolutePath path)
    {
        if (_upstream.DirectoryExists(path)) return true;
        return SourceHasAnyUnder(path);
    }

    protected override void InternalDeleteDirectory(AbsolutePath path, bool recursive)
    {
        if (_upstream.DirectoryExists(path))
        {
            _upstream.DeleteDirectory(path, recursive);
            return;
        }
        // Directory only represented by read-only sources
        throw new IOException($"Cannot delete read-only directory: {path}");
    }

    protected override bool InternalFileExists(AbsolutePath path)
    {
        if (_upstream.FileExists(path)) return true;
        return TryResolveSource(path, out var src, out var rel) && src.Exists(rel);
    }

    protected override void InternalDeleteFile(AbsolutePath path)
    {
        if (_upstream.FileExists(path))
        {
            _upstream.DeleteFile(path);
            return;
        }

        // Exists only in read-only sources
        throw new IOException($"Cannot delete read-only file: {path}");
    }

    protected override void InternalMoveFile(AbsolutePath source, AbsolutePath dest, bool overwrite)
    {
        if (_upstream.FileExists(source))
        {
            _upstream.MoveFile(source, dest, overwrite);
            return;
        }

        // Copy-on-write semantics if moving a read-only file to a writable location is requested via a write op
        if (TryResolveSource(source, out var src, out var rel))
        {
            EnsureCopiedToUpstream(source, src, rel);
            _upstream.MoveFile(source, dest, overwrite);
            return;
        }

        throw new FileNotFoundException($"File not found: {source}");
    }

    protected override unsafe MemoryMappedFileHandle InternalCreateMemoryMappedFile(AbsolutePath absPath, FileMode mode, MemoryMappedFileAccess access, ulong fileSize)
    {
        // If upstream has or we need write access, ensure upstream and delegate
        if (_upstream.FileExists(absPath) || access != MemoryMappedFileAccess.Read)
        {
            if (!_upstream.FileExists(absPath) && TryResolveSource(absPath, out var s, out var r) && access != MemoryMappedFileAccess.Read)
            {
                EnsureCopiedToUpstream(absPath, s, r);
            }
            return _upstream.CreateMemoryMappedFile(absPath, mode, access, fileSize);
        }

        // Read-only mapping from source
        if (!TryResolveSource(absPath, out var src, out var rel))
            throw new FileNotFoundException($"File not found: {absPath}");

        using var stream = src.OpenRead(rel);
        var len = (int)stream.Length;
        var buffer = GC.AllocateUninitializedArray<byte>(len);
        stream.ReadExactly(buffer, 0, len);
        var pin = new Pin<byte>(buffer);
        return new MemoryMappedFileHandle(pin.Pointer, (nuint)buffer.Length, pin);
    }

    private IEnumerable<AbsolutePath> EnumerateSourceFilesUnder(AbsolutePath directory, bool recursive)
    {
        foreach (var src in _sources)
        {
            foreach (var rel in src.EnumerateFiles())
            {
                var abs = src.MountPoint / rel;
                if (recursive)
                {
                    if (abs.InFolder(directory) || abs.Parent == directory)
                        yield return abs.WithFileSystem(this);
                }
                else
                {
                    if (abs.Parent == directory)
                        yield return abs.WithFileSystem(this);
                }
            }
        }
    }

    private IEnumerable<AbsolutePath> EnumerateSourceDirectoriesUnder(AbsolutePath directory, bool recursive)
    {
        var set = new HashSet<AbsolutePath>();
        foreach (var src in _sources)
        {
            foreach (var rel in src.EnumerateFiles())
            {
                var abs = src.MountPoint / rel;
                foreach (var parent in abs.GetAllParents())
                {
                    if (!recursive && parent != abs.Parent) continue;
                    if (directory == parent || abs.InFolder(directory))
                    {
                        if (parent == abs) continue; // skip the file itself
                        if (parent == abs.GetRootDirectory()) continue;
                        var p = parent.WithFileSystem(this);
                        if (p.InFolder(directory) || p == directory)
                        {
                            if (set.Add(p)) yield return p;
                        }
                    }
                }
            }
        }
    }

    private bool TryResolveSource(AbsolutePath path, out IReadOnlyFileSource source, out RelativePath relative)
    {
        foreach (var s in _sources)
        {
            if (path.InFolder(s.MountPoint) || path == s.MountPoint || path.Parent == s.MountPoint)
            {
                var rel = path.RelativeTo(s.MountPoint);
                if (rel == RelativePath.Empty && path == s.MountPoint)
                {
                    // path refers to mountpoint itself; no file
                    continue;
                }
                if (s.Exists(rel))
                {
                    source = s;
                    relative = rel;
                    return true;
                }
            }
        }
        source = default!;
        relative = default;
        return false;
    }

    private bool SourceHasAnyUnder(AbsolutePath directory)
    {
        foreach (var s in _sources)
        {
            foreach (var rel in s.EnumerateFiles())
            {
                var abs = s.MountPoint / rel;
                if (abs.InFolder(directory) || abs.Parent == directory)
                    return true;
            }
        }
        return false;
    }

    private void EnsureCopiedToUpstream(AbsolutePath abs, IReadOnlyFileSource source, RelativePath rel)
    {
        if (_upstream.FileExists(abs)) return;
        _upstream.CreateDirectory(abs.Parent);
        using var read = source.OpenRead(rel);
        using var write = _upstream.CreateFile(abs);
        read.CopyTo(write);
    }

    private sealed class VirtualReadOnlyFileEntry : IFileEntry
    {
        public IFileSystem FileSystem { get; set; }
        public AbsolutePath Path { get; }
        public Size Size { get; }
        public DateTime LastWriteTime { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsReadOnly { get; set; }

        public VirtualReadOnlyFileEntry(IFileSystem fs, AbsolutePath path, ulong size)
        {
            FileSystem = fs;
            Path = path;
            Size = Size.From(size);
            LastWriteTime = DateTime.UnixEpoch;
            CreationTime = DateTime.UnixEpoch;
            IsReadOnly = true;
        }

        public FileVersionInfo GetFileVersionInfo() => new(new Version(0,0,0,0), new Version(0,0,0,0), null, null);
    }

    private sealed class VirtualDirectoryEntry : IDirectoryEntry { }
}


