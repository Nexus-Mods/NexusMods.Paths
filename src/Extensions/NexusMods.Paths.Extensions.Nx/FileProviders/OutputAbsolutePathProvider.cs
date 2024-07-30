using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using NexusMods.Archives.Nx.FileProviders.FileData;
using NexusMods.Archives.Nx.Headers.Managed;
using NexusMods.Archives.Nx.Interfaces;
using NexusMods.Paths.Extensions.Nx.FileProviders.FileData;

namespace NexusMods.Paths.Extensions.Nx.FileProviders;

/// <summary>
///     A provider for creating <see cref="IFileData" /> instances which allow
///     the user to output information to an absolute path.
/// </summary>
public class OutputAbsolutePathProvider : IOutputDataProvider
{
    /// <inheritdoc />
    public string RelativePath { get; }

    /// <inheritdoc />
    public FileEntry Entry { get; }

    /// <summary>
    ///     Full path to the file.
    /// </summary>
    public AbsolutePath FullPath { get; }

    private readonly MemoryMappedFileHandle? _mappedFileHandle;
    private bool _isDisposed;
    private readonly bool _isEmpty;

    /// <summary>
    ///     Initializes outputting a file to an absolute path.
    /// </summary>
    /// <param name="fullPath">The absolute path to output the file.</param>
    /// <param name="relativePath">The relative path of the file (context from the Nx archive).</param>
    /// <param name="entry">The individual file entry (context from the Nx archive).</param>
    public OutputAbsolutePathProvider(AbsolutePath fullPath, string relativePath, FileEntry entry)
    {
        RelativePath = relativePath;
        Entry = entry;
        FullPath = fullPath;

    TryCreate:
        try
        {
            if (entry.DecompressedSize <= 0)
            {
                using var _ = FullPath.FileSystem.CreateFile(FullPath);
                _isEmpty = true;
                return;
            }

            // Ensure the directory exists
            FullPath.FileSystem.CreateDirectory(FullPath.Parent);

            // Delete the file if it exists to ensure we start with an empty file
            if (FullPath.FileSystem.FileExists(FullPath))
                FullPath.FileSystem.DeleteFile(FullPath);

            // Create the memory mapped file
            _mappedFileHandle = FullPath.FileSystem.CreateMemoryMappedFile(FullPath, FileMode.CreateNew, MemoryMappedFileAccess.ReadWrite, entry.DecompressedSize);
        }
        catch (DirectoryNotFoundException)
        {
            // This is written this way because explicit check is slow.
            FullPath.FileSystem.CreateDirectory(FullPath.Parent);
            goto TryCreate;
        }
    }

    /// <inheritdoc />
    public IFileData GetFileData(ulong start, ulong length)
    {
        if (_isEmpty)
            return new ArrayFileData(Array.Empty<byte>(), 0, 0);

        return new PathsMemoryMappedFileData(_mappedFileHandle!.Value, start, length, false);
    }

    /// <inheritdoc />
    ~OutputAbsolutePathProvider() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _mappedFileHandle?.Dispose();
        GC.SuppressFinalize(this);
    }
}
