using NexusMods.Archives.Nx.Interfaces;
using NexusMods.Paths.Extensions.Nx.FileProviders.FileData;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace NexusMods.Paths.Extensions.Nx.FileProviders;

/// <summary>
///    A provider for creating <see cref="IFileData" /> instances from an absolute path
/// </summary>
public class FromAbsolutePathProvider : IFileDataProvider
{
    /// <summary>
    ///     The full path to the file from which the data will be fetched.
    /// </summary>
    public required AbsolutePath FilePath { get; init; }

    /// <inheritdoc />
    public IFileData GetFileData(ulong start, ulong length)
    {
        // TODO: This could probably be better, as it's unoptimal for chunked files.
        // Ideally the file should be opened once in the provider and then calls in GetFileData
        // could work on slices of the larger MMF.
        var fileSystem = FilePath.FileSystem;
        var handle = fileSystem.CreateMemoryMappedFile(FilePath, FileMode.Open, MemoryMappedFileAccess.Read, 0);
        return new PathsMemoryMappedFileData(handle, start, length);
    }
}
