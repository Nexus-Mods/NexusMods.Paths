using System;

namespace NexusMods.Paths.FileProviders;

/// <summary>
/// Represents a chunk of read-only file data. Implementations may wrap memory-mapped data,
/// array segments, or other non-stream sources.
/// </summary>
public interface IFileDataChunk : IDisposable
{
    /// <summary>
    /// Read-only view over the data for this chunk.
    /// </summary>
    ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    /// Length of the data available in <see cref="Data"/>.
    /// </summary>
    ulong DataLength { get; }
}
