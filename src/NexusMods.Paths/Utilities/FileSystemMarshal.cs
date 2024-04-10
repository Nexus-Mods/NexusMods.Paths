namespace NexusMods.Paths.Utilities;

/// <summary>
///     This is a suite of low level methods that provide 'escape hatches' and
///     other utilities for the file system abstraction where lower level access
///     is required.
/// </summary>
/// <remarks>
///     This is named after classes like
///     <a href="https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.collectionsmarshal?view=net-8.0">CollectionsMarshal</a>
///     in the .NET Runtime.
/// </remarks>
public static class FileSystemMarshal
{
    /// <summary>
    ///     Tries to resolve the path of the file to a path in the real
    ///     FileSystem.
    ///
    ///     This acts as an 'escape hatch' for using real FileSystem functionality
    ///     which is not yet available as part of the <see cref="IFileSystem"/> abstraction.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="fullFilePath">File path to the raw file on the FileSystem.</param>
    /// <returns>
    ///     True if the path was successfully resolved to a real file on disk,
    ///     otherwise false.
    /// </returns>
    /// <remarks>
    ///     If this method returns 'false' you should fall back to a slower implementation
    ///     that works with the <see cref="IFileSystem"/> abstraction, or throw
    ///     an exception.
    /// </remarks>
    public static bool TryResolveRealFilesystemPath(AbsolutePath path, out string? fullFilePath)
    {
        // Only paths from the Real 'FileSystem' can be resolved here.
        if (path.FileSystem is not FileSystem realFs)
        {
            fullFilePath = null;
            return false;
        }

        // Resolve the path.
        fullFilePath = realFs.GetMappedPath(path).GetFullPath();
        return true;
    }
}
