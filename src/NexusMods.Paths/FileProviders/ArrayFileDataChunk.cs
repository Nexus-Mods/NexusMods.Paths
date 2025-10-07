using System;

namespace NexusMods.Paths.FileProviders;

/// <summary>
/// Simple in-memory chunk implementation over a byte array.
/// </summary>
public sealed class ArrayFileDataChunk : IFileDataChunk
{
    private readonly byte[] _buffer;
    private readonly int _offset;
    private readonly int _count;
    private bool _disposed;

    public ArrayFileDataChunk(byte[] buffer, int offset, int count)
    {
        _buffer = buffer;
        _offset = offset;
        _count = count < 0 ? 0 : count;
    }

    public ReadOnlyMemory<byte> Data => new ReadOnlyMemory<byte>(_buffer, _offset, _count);

    public ulong DataLength => (ulong)_count;

    public void Dispose()
    {
        _disposed = true; // nothing to dispose, but allow idempotent calls
    }
}
