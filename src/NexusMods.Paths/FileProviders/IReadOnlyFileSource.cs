﻿using System.Collections.Generic;
using System.IO;

namespace NexusMods.Paths.FileProviders;

/// <summary>
/// Defines a read-only file source that exposes files under a mount point using relative paths.
/// Implementations are not required to return actual FileStreams and may serve data in chunks.
/// </summary>
public interface IReadOnlyFileSource
{
    /// <summary>
    /// Where this source is conceptually mounted in an <see cref="IFileSystem"/>.
    /// </summary>
    AbsolutePath MountPoint { get; }

    /// <summary>
    /// Enumerates files exposed by this source as relative paths.
    /// </summary>
    IEnumerable<RelativePath> EnumerateFiles();

    /// <summary>
    /// Returns true if a file exists at the given relative path.
    /// </summary>
    bool Exists(RelativePath relativePath);

    /// <summary>
    /// Opens a read-only stream for the given relative path.
    /// </summary>
    Stream OpenRead(RelativePath relativePath);

    /// <summary>
    /// Provides a chunked view into the requested file data.
    /// </summary>
    IFileDataChunk GetFileData(RelativePath relativePath, ulong start, ulong length);
}
