using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NexusMods.Paths.Utilities;
using Reloaded.Memory.Extensions;

namespace NexusMods.Paths;

/// <summary>
/// Abstract class for implementations of <see cref="IFileSystem"/> that
/// provides helper functions and reduces code duplication.
/// </summary>
[PublicAPI]
public abstract class BaseFileSystem : IFileSystem
{
    private readonly Dictionary<AbsolutePath, AbsolutePath> _pathMappings = new();
    private readonly Dictionary<KnownPath, AbsolutePath> _knownPathMappings = new();

    private readonly bool _hasPathMappings;
    private readonly bool _convertCrossPlatformPaths;

    /// <summary>
    /// Constructor.
    /// </summary>
    protected BaseFileSystem() { }

    /// <summary>
    /// Constructor that accepts path mappings.
    /// </summary>
    protected BaseFileSystem(
        IOSInformation os,
        Dictionary<AbsolutePath, AbsolutePath> pathMappings,
        Dictionary<KnownPath, AbsolutePath> knownPathMappings,
        bool convertCrossPlatformPaths)
    {
        OS = os;
        _pathMappings = pathMappings;
        _knownPathMappings = knownPathMappings;
        _hasPathMappings = _pathMappings.Any();
        _convertCrossPlatformPaths = convertCrossPlatformPaths;
    }

    internal AbsolutePath GetMappedPath(AbsolutePath originalPath)
    {
        // fast exit
        if (!_hasPathMappings) return originalPath;

        // direct mapping
        if (_pathMappings.TryGetValue(originalPath, out var mappedPath))
            return mappedPath;

        // check if the path has already been mapped
        if (_pathMappings
                .Where(kv => kv.Value is not { Directory: "/", FileName: "" })
                .FirstOrDefault(kv => originalPath.InFolder(kv.Value)).Key != default)
        {
            return originalPath;
        }

        // indirect mapping via parent directory
        var (originalParentDirectory, newParentDirectory) = _pathMappings
            .FirstOrDefault(kv => originalPath.InFolder(kv.Key));

        if (newParentDirectory == default) return originalPath;

        var relativePath = originalPath.RelativeTo(originalParentDirectory);
        var newPath = newParentDirectory / relativePath;

        return newPath;
    }

    #region IFileStream Implementation

    /// <inheritdoc/>
    public IOSInformation OS { get; } = OSInformation.Shared;

    /// <inheritdoc/>
    public abstract IFileSystem CreateOverlayFileSystem(
        Dictionary<AbsolutePath, AbsolutePath> pathMappings,
        Dictionary<KnownPath, AbsolutePath> knownPathMappings,
        bool convertCrossPlatformPaths = false);

    /// <inheritdoc/>
    public bool HasKnownPath(KnownPath knownPath)
    {
        Debug.Assert(Enum.IsDefined(knownPath));

        return knownPath switch
        {
            KnownPath.EntryDirectory => true,
            KnownPath.CurrentDirectory => true,
            KnownPath.CommonApplicationDataDirectory => true,
            KnownPath.ProgramFilesDirectory => IsWindowsOrMapped(),
            KnownPath.ProgramFilesX86Directory => IsWindowsOrMapped(),
            KnownPath.CommonProgramFilesDirectory => IsWindowsOrMapped(),
            KnownPath.CommonProgramFilesX86Directory => IsWindowsOrMapped(),
            KnownPath.TempDirectory => true,
            KnownPath.HomeDirectory => true,
            KnownPath.ApplicationDataDirectory => true,
            KnownPath.LocalApplicationDataDirectory => true,
            KnownPath.MyDocumentsDirectory => true,
            KnownPath.MyGamesDirectory => true,

            KnownPath.XDG_CONFIG_HOME => OS.IsLinux,
            KnownPath.XDG_CACHE_HOME => OS.IsLinux,
            KnownPath.XDG_DATA_HOME => OS.IsLinux,
            KnownPath.XDG_STATE_HOME => OS.IsLinux,
            KnownPath.XDG_RUNTIME_DIR => OS.IsLinux,
        };

        bool IsWindowsOrMapped()
        {
            return OS.IsWindows || _knownPathMappings.ContainsKey(knownPath);
        }
    }

    /// <inheritdoc/>
    public virtual AbsolutePath GetKnownPath(KnownPath knownPath)
    {
        Debug.Assert(Enum.IsDefined(knownPath));

        if (_knownPathMappings.TryGetValue(knownPath, out var mappedPath))
            return mappedPath;

        // NOTE(erri120): if you change this method, make sure to update the docs in the KnownPath enum.
        var path = knownPath switch
        {
            KnownPath.EntryDirectory => FromUnsanitizedFullPath(AppContext.BaseDirectory),
            KnownPath.CurrentDirectory => FromUnsanitizedFullPath(Environment.CurrentDirectory),

            KnownPath.CommonApplicationDataDirectory => FromUnsanitizedFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)),
            KnownPath.ProgramFilesDirectory => ThisOrDefault(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)),
            KnownPath.ProgramFilesX86Directory => ThisOrDefault(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)),
            KnownPath.CommonProgramFilesDirectory => ThisOrDefault(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles)),
            KnownPath.CommonProgramFilesX86Directory => ThisOrDefault(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86)),

            KnownPath.TempDirectory => FromUnsanitizedFullPath(Path.GetTempPath()),
            KnownPath.HomeDirectory => FromUnsanitizedFullPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)),
            KnownPath.ApplicationDataDirectory => FromUnsanitizedFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            KnownPath.LocalApplicationDataDirectory => FromUnsanitizedFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
            KnownPath.MyDocumentsDirectory => FromUnsanitizedFullPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
            KnownPath.MyGamesDirectory => FromUnsanitizedDirectoryAndFileName(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games"),

            KnownPath.XDG_CONFIG_HOME => GetXDGBaseDirectory($"{nameof(KnownPath.XDG_CONFIG_HOME)}", static fs => fs.GetKnownPath(KnownPath.HomeDirectory) / ".config"),
            KnownPath.XDG_CACHE_HOME => GetXDGBaseDirectory($"{nameof(KnownPath.XDG_CACHE_HOME)}", static fs => fs.GetKnownPath(KnownPath.HomeDirectory) / ".cache"),
            KnownPath.XDG_DATA_HOME => GetXDGBaseDirectory($"{nameof(KnownPath.XDG_DATA_HOME)}", static fs => fs.GetKnownPath(KnownPath.HomeDirectory) / ".local" / "share"),
            KnownPath.XDG_STATE_HOME => GetXDGBaseDirectory($"{nameof(KnownPath.XDG_STATE_HOME)}", static fs => fs.GetKnownPath(KnownPath.HomeDirectory) / ".local" / "state"),
            KnownPath.XDG_RUNTIME_DIR => GetXDGBaseDirectory($"{nameof(KnownPath.XDG_RUNTIME_DIR)}", static fs => fs.GetKnownPath(KnownPath.TempDirectory)),
        };

        Debug.Assert(path != default, $"{nameof(GetKnownPath)} returns 'default' for {nameof(KnownPath)} '{knownPath}'. You forgot to add a mapping for this {nameof(KnownPath)}!");
        return path;

        // ReSharper disable once InconsistentNaming
        AbsolutePath GetXDGBaseDirectory(string environmentVariable, Func<IFileSystem, AbsolutePath> defaultFunc)
        {
            if (!OS.IsLinux) OS.ThrowUnsupported();

            var value = Environment.GetEnvironmentVariable(environmentVariable, EnvironmentVariableTarget.Process);
            return value is null ? defaultFunc(this) : FromUnsanitizedFullPath(value);
        }

        AbsolutePath ThisOrDefault(string fullPath)
        {
            return string.IsNullOrWhiteSpace(fullPath) ? default : FromUnsanitizedFullPath(fullPath);
        }
    }

    /// <summary>
    /// Returns a dictionary with the required path mappings and <see cref="KnownPath"/> mappings for Wine.
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <param name="rootDirectory"></param>
    /// <param name="newHomeDirectory"></param>
    /// <returns></returns>
    public static (Dictionary<AbsolutePath, AbsolutePath> pathMappings, Dictionary<KnownPath, AbsolutePath> knownPathMappings) CreateWinePathMappings(
        IFileSystem fileSystem, AbsolutePath rootDirectory, AbsolutePath newHomeDirectory)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            throw new PlatformNotSupportedException();

        // TODO: these mappings can be changed using winecfg and are stored inside the "dosdevices" folder
        // NOTE(erri120): I need to revisit this entire function at some point and figure out a better way to handle path mappings
        var pathMappings = new Dictionary<AbsolutePath, AbsolutePath>
        {
            { fileSystem.FromUnsanitizedFullPath("C:/"), rootDirectory },
            { fileSystem.FromUnsanitizedFullPath("Z:/"), fileSystem.FromUnsanitizedFullPath("/") },
        };

        var knownPaths = Enum.GetValues<KnownPath>();
        var knownPathMappings = new Dictionary<KnownPath, AbsolutePath>(knownPaths.Length);
        foreach (var knownPath in knownPaths)
        {
            var newPath = knownPath switch
            {
                KnownPath.EntryDirectory => fileSystem.GetKnownPath(knownPath),
                KnownPath.CurrentDirectory => fileSystem.GetKnownPath(knownPath),

                KnownPath.CommonApplicationDataDirectory => rootDirectory / "ProgramData",
                KnownPath.ProgramFilesDirectory => rootDirectory / "Program Files",
                KnownPath.ProgramFilesX86Directory => rootDirectory / "Program Files (x86)",
                KnownPath.CommonProgramFilesDirectory => rootDirectory / "Program Files/Common Files",
                KnownPath.CommonProgramFilesX86Directory => rootDirectory / "Program Files (x86)/Common Files",

                KnownPath.HomeDirectory => newHomeDirectory,
                KnownPath.MyDocumentsDirectory => newHomeDirectory / "Documents",
                KnownPath.MyGamesDirectory => newHomeDirectory / "Documents/My Games",
                KnownPath.LocalApplicationDataDirectory => newHomeDirectory / "AppData/Local",
                KnownPath.ApplicationDataDirectory => newHomeDirectory / "AppData/Roaming",
                KnownPath.TempDirectory => newHomeDirectory / "AppData/Local/Temp",

                _ => default
            };

            if (newPath == default) continue;
            knownPathMappings[knownPath] = newPath;
        }

        return (pathMappings, knownPathMappings);
    }

    /// <inheritdoc/>
    public AbsolutePath FromUnsanitizedFullPath(string unsanitizedFullPath)
    {
        var sanitized = PathHelpers.Sanitize(unsanitizedFullPath);
        var path = AbsolutePath.FromSanitizedFullPath(sanitized, this);
        var mappedPath = GetMappedPath(path);
        return mappedPath;
    }

    /// <inheritdoc/>
    public AbsolutePath FromUnsanitizedDirectoryAndFileName(string directoryPath, string fileName)
    {
        return AbsolutePath.FromUnsanitizedDirectoryAndFileName(directoryPath, fileName, this);
    }

    /// <inheritdoc/>
    public IFileEntry GetFileEntry(AbsolutePath path)
        => InternalGetFileEntry(GetMappedPath(path));

    /// <inheritdoc/>
    public IDirectoryEntry GetDirectoryEntry(AbsolutePath path)
        => InternalGetDirectoryEntry(GetMappedPath(path));

    /// <inheritdoc/>
    public IEnumerable<AbsolutePath> EnumerateRootDirectories()
    {
        if (OS.IsUnix())
        {
            yield return AbsolutePath.FromSanitizedFullPath("/", this);
        }

        // go through the valid drive letters on Windows
        // or on Linux with cross platform path conversion enabled, in case
        // the current file system is an overlay file system for Wine
        if (OS.IsWindows || (OS.IsLinux && _convertCrossPlatformPaths))
        {
            for (var i = (uint)'A'; i <= 'Z'; i++)
            {
                var driveLetter = (char)i;
                var path = FromUnsanitizedFullPath($"{driveLetter}:/");
                if (DirectoryExists(path)) yield return path;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<AbsolutePath> EnumerateFiles(AbsolutePath directory, string pattern = "*", bool recursive = true)
        => InternalEnumerateFiles(GetMappedPath(directory), pattern, recursive);

    /// <inheritdoc/>
    public IEnumerable<AbsolutePath> EnumerateDirectories(AbsolutePath directory, string pattern = "*", bool recursive = true)
        => InternalEnumerateDirectories(GetMappedPath(directory), pattern, recursive);

    /// <inheritdoc/>
    public IEnumerable<IFileEntry> EnumerateFileEntries(AbsolutePath directory, string pattern = "*", bool recursive = true)
        => InternalEnumerateFileEntries(GetMappedPath(directory), pattern, recursive);

    /// <inheritdoc/>
    public Stream ReadFile(AbsolutePath path) => OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    /// <inheritdoc/>
    public Stream WriteFile(AbsolutePath path) => OpenFile(path, FileMode.Open, FileAccess.Write, FileShare.Read);

    /// <inheritdoc/>
    public Stream CreateFile(AbsolutePath path) => OpenFile(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

    /// <inheritdoc/>
    public Stream OpenFile(AbsolutePath path, FileMode mode, FileAccess access, FileShare share)
        => InternalOpenFile(GetMappedPath(path), mode, access, share);

    /// <inheritdoc/>
    public void CreateDirectory(AbsolutePath path)
        => InternalCreateDirectory(GetMappedPath(path));

    /// <inheritdoc/>
    public bool DirectoryExists(AbsolutePath path)
        => InternalDirectoryExists(GetMappedPath(path));

    /// <inheritdoc/>
    public void DeleteDirectory(AbsolutePath path, bool recursive)
        => InternalDeleteDirectory(GetMappedPath(path), recursive);

    /// <inheritdoc/>
    public bool FileExists(AbsolutePath path)
        => InternalFileExists(GetMappedPath(path));

    /// <inheritdoc/>
    public void DeleteFile(AbsolutePath path)
        => InternalDeleteFile(GetMappedPath(path));

    /// <inheritdoc/>
    public void MoveFile(AbsolutePath source, AbsolutePath dest, bool overwrite)
        => InternalMoveFile(GetMappedPath(source), GetMappedPath(dest), overwrite);

    /// <inheritdoc />
    public abstract int ReadBytesRandomAccess(AbsolutePath path, Span<byte> bytes, long offset);

    /// <inheritdoc />
    public abstract Task<int> ReadBytesRandomAccessAsync(AbsolutePath path, Memory<byte> bytes, long offset, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public async Task<byte[]> ReadAllBytesAsync(AbsolutePath path, CancellationToken cancellationToken = default)
    {
        await using var s = ReadFile(path);
        var length = s.Length;
        var bytes = GC.AllocateUninitializedArray<byte>((int)length);
        await s.ReadAtLeastAsync(bytes, bytes.Length, false, cancellationToken);
        return bytes;
    }

    /// <inheritdoc/>
    public async Task<string> ReadAllTextAsync(AbsolutePath path, CancellationToken cancellationToken = default)
    {
        return Encoding.UTF8.GetString(await ReadAllBytesAsync(path, cancellationToken));
    }

    /// <inheritdoc/>
    public async Task WriteAllBytesAsync(AbsolutePath path, byte[] data, CancellationToken cancellationToken = default)
    {
        await using var fs = CreateFile(path);
        await fs.WriteAsync(data, CancellationToken.None);
    }

    /// <inheritdoc/>
    public async Task WriteAllTextAsync(AbsolutePath path, string text, CancellationToken cancellationToken = default)
    {
        await using var fs = CreateFile(path);
        await fs.WriteAsync(Encoding.UTF8.GetBytes(text), cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteAllText(AbsolutePath path, string text)
    {
        using var fs = CreateFile(path);
        fs.Write(Encoding.UTF8.GetBytes(text));
    }

    /// <inheritdoc/>
    public async Task WriteAllLinesAsync(AbsolutePath path, [InstantHandle(RequireAwait = true)] IEnumerable<string> lines, CancellationToken cancellationToken = default)
    {
        await using var fs = CreateFile(path);
        await using var sw = new StreamWriter(fs);
        foreach (var line in lines)
        {
            await sw.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }

    /// <inheritdoc/>
    public virtual void SetUnixFileMode(AbsolutePath absolutePath, UnixFileMode flags)
    {
        // No-op on most implementations, so not calling internal.
    }

    /// <inheritdoc/>
    public virtual UnixFileMode GetUnixFileMode(AbsolutePath absolutePath)
    {
        // No-op on most implementations, so not calling internal.
        return default;
    }

    /// <inheritdoc/>
    public MemoryMappedFileHandle CreateMemoryMappedFile(AbsolutePath absPath, FileMode mode, MemoryMappedFileAccess access, ulong fileSize)
    {
        return InternalCreateMemoryMappedFile(GetMappedPath(absPath), mode, access, fileSize);
    }

    #endregion

    #region Abstract Methods

    /// <inheritdoc cref="IFileSystem.GetFileEntry"/>
    protected abstract IFileEntry InternalGetFileEntry(AbsolutePath path);

    /// <inheritdoc cref="IFileSystem.GetDirectoryEntry"/>
    protected abstract IDirectoryEntry InternalGetDirectoryEntry(AbsolutePath path);

    /// <inheritdoc cref="IFileSystem.EnumerateFiles"/>
    protected abstract IEnumerable<AbsolutePath> InternalEnumerateFiles(AbsolutePath directory, string pattern, bool recursive);

    /// <inheritdoc cref="IFileSystem.EnumerateFiles"/>
    protected abstract IEnumerable<AbsolutePath> InternalEnumerateDirectories(AbsolutePath directory, string pattern, bool recursive);

    /// <inheritdoc cref="IFileSystem.EnumerateFiles"/>
    protected abstract IEnumerable<IFileEntry> InternalEnumerateFileEntries(AbsolutePath directory, string pattern, bool recursive);

    /// <inheritdoc cref="IFileSystem.OpenFile"/>
    protected abstract Stream InternalOpenFile(AbsolutePath path, FileMode mode, FileAccess access, FileShare share);

    /// <inheritdoc cref="IFileSystem.CreateDirectory"/>
    protected abstract void InternalCreateDirectory(AbsolutePath path);

    /// <inheritdoc cref="IFileSystem.DirectoryExists"/>
    protected abstract bool InternalDirectoryExists(AbsolutePath path);

    /// <inheritdoc cref="IFileSystem.DeleteDirectory"/>
    protected abstract void InternalDeleteDirectory(AbsolutePath path, bool recursive);

    /// <inheritdoc cref="IFileSystem.FileExists"/>
    protected abstract bool InternalFileExists(AbsolutePath path);

    /// <inheritdoc cref="IFileSystem.DeleteFile"/>
    protected abstract void InternalDeleteFile(AbsolutePath path);

    /// <inheritdoc cref="IFileSystem.MoveFile"/>
    protected abstract void InternalMoveFile(AbsolutePath source, AbsolutePath dest, bool overwrite);

    /// <inheritdoc cref="IFileSystem.CreateMemoryMappedFile"/>
    protected abstract MemoryMappedFileHandle InternalCreateMemoryMappedFile(AbsolutePath absPath, FileMode mode, MemoryMappedFileAccess access, ulong fileSize);
    #endregion
}
