using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMods.Paths.FileProviders;

/// <summary>
/// Read-only file source backed by in-memory byte arrays. Useful for tests.
/// </summary>
public sealed class InMemoryReadOnlyFileSource : IReadOnlyFileSource
{
    private readonly Dictionary<RelativePath, byte[]> _files;

    public InMemoryReadOnlyFileSource(AbsolutePath mountPoint, Dictionary<RelativePath, byte[]> files)
    {
        MountPoint = mountPoint;
        _files = files;
    }

    public AbsolutePath MountPoint { get; }

    public IEnumerable<RelativePath> EnumerateFiles() => _files.Keys;

    public bool Exists(RelativePath relativePath) => _files.ContainsKey(relativePath);

    public Stream OpenRead(RelativePath relativePath)
    {
        if (!_files.TryGetValue(relativePath, out var data))
            throw new FileNotFoundException($"File not found: {relativePath}");

        return new ReadOnlyChunkedStream(data);
    }

    public IChunkedStreamSource GetChunkedSource(RelativePath relativePath, int chunkSize = 4096)
    {
        if (!_files.TryGetValue(relativePath, out var data))
            throw new FileNotFoundException($"File not found: {relativePath}");
        return new InMemoryChunkedSource(data, chunkSize);
    }

    private sealed class ReadOnlyChunkedStream : Stream
    {
        private readonly byte[] _buffer;
        private long _position;

        public ReadOnlyChunkedStream(byte[] buffer) => _buffer = buffer;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _buffer.LongLength;
        public override long Position { get => _position; set => _position = value; }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count <= 0) return 0;
            var remaining = (int)Math.Min(count, Length - Position);
            if (remaining <= 0) return 0;
            Array.Copy(_buffer, (int)Position, buffer, offset, remaining);
            Position += remaining;
            return remaining;
        }

        public override int Read(Span<byte> destination)
        {
            var remaining = (int)Math.Min(destination.Length, Length - Position);
            if (remaining <= 0) return 0;
            _buffer.AsSpan((int)Position, remaining).CopyTo(destination);
            Position += remaining;
            return remaining;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPos = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            if (newPos < 0) throw new IOException("Attempted to seek before beginning of stream.");
            Position = newPos;
            return Position;
        }

        public override void SetLength(long value) => throw new NotSupportedException("Stream is read-only.");
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("Stream is read-only.");
        public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException("Stream is read-only.");
    }

    private sealed class InMemoryChunkedSource : IChunkedStreamSource
    {
        private readonly byte[] _data;
        private readonly int _chunkSize;
        public InMemoryChunkedSource(byte[] data, int chunkSize)
        {
            _data = data;
            _chunkSize = Math.Max(1, chunkSize);
        }

        public Size Size => Size.FromLong(_data.LongLength);
        public ulong ChunkCount => (ulong)Math.Ceiling((double)_data.LongLength / _chunkSize);
        public ulong GetOffset(ulong chunkIndex) => (ulong)((long)chunkIndex * _chunkSize);
        public int GetChunkSize(ulong chunkIndex)
        {
            var start = (long)GetOffset(chunkIndex);
            var remaining = Math.Max(0, _data.Length - (int)start);
            return Math.Min(_chunkSize, remaining);
        }
        public Task ReadChunkAsync(Memory<byte> buffer, ulong chunkIndex, CancellationToken token = default)
        {
            ReadChunk(buffer.Span, chunkIndex);
            return Task.CompletedTask;
        }
        public void ReadChunk(Span<byte> buffer, ulong chunkIndex)
        {
            var size = GetChunkSize(chunkIndex);
            _data.AsSpan((int)GetOffset(chunkIndex), size).CopyTo(buffer);
        }
    }
}
