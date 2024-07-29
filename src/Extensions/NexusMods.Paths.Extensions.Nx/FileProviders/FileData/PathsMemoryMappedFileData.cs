using System;
using NexusMods.Archives.Nx.Interfaces;

namespace NexusMods.Paths.Extensions.Nx.FileProviders.FileData;

/// <summary>
///     Provides access to data of a Paths memory-mapped file.
/// </summary>
public unsafe class PathsMemoryMappedFileData : IFileData
{
    private readonly MemoryMappedFileHandle _memoryMappedFileHandle;
    private readonly bool _disposeHandle;
    private bool _disposed;

    /// <inheritdoc />
    public byte* Data { get; }

    /// <inheritdoc />
    public ulong DataLength { get; }

    /// <summary>
    ///     Paths memory mapped file data.
    /// </summary>
    /// <param name="handle">The handle to use</param>
    /// <param name="start">The start offset in the file</param>
    /// <param name="length">The length of the data to map</param>
    /// <param name="disposeHandle">Disposes the handle on close.</param>
    public PathsMemoryMappedFileData(MemoryMappedFileHandle handle, ulong start, ulong length, bool disposeHandle = true)
    {
        _memoryMappedFileHandle = handle;
        _disposeHandle = disposeHandle;
        if (start >= handle.Length)
        {
            Data = handle.Pointer;
            DataLength = 0;
        }
        else
        {
            Data = handle.Pointer + start;
            DataLength = Math.Min(length, handle.Length - start);
        }
    }

    /// <inheritdoc />
    ~PathsMemoryMappedFileData() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_disposeHandle)
            _memoryMappedFileHandle.Dispose();

        GC.SuppressFinalize(this);
    }
}
