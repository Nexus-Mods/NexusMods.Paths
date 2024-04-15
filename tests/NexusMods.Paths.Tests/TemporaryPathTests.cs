using NexusMods.Paths.TestingHelpers;

namespace NexusMods.Paths.Tests;

public class TemporaryPathTests
{
    [Theory, AutoFileSystem]
    public void FilesAreInBaseDirectory(InMemoryFileSystem fs, TemporaryFileManager manager)
    {
        using var path = manager.CreateFile();
        path.Path.InFolder(fs.GetKnownPath(KnownPath.TempDirectory)).Should().BeTrue();
    }

    [Theory, AutoFileSystem]
    public async Task FilesAreDeletedWhenFlagged(InMemoryFileSystem fs, TemporaryFileManager manager)
    {
        var deletedPath = manager.CreateFile();
        await fs.WriteAllTextAsync(deletedPath, "File A");

        var notDeletedPath = manager.CreateFile(deleteOnDispose: false);
        await fs.WriteAllTextAsync(notDeletedPath, "File B");

        fs.FileExists(deletedPath).Should().BeTrue();
        fs.FileExists(notDeletedPath).Should().BeTrue();

        await deletedPath.DisposeAsync();
        await notDeletedPath.DisposeAsync();

        fs.FileExists(deletedPath).Should().BeFalse();
        fs.FileExists(notDeletedPath).Should().BeTrue();
    }

    [Theory, AutoFileSystem]
    public async Task DirectoriesAreDeleted(InMemoryFileSystem fs, TemporaryFileManager manager)
    {
        var tempDirPath = manager.CreateFolder();

        // Create some children
        var childFileA = tempDirPath.Path.Combine("File A");
        fs.CreateFile(childFileA);

        var childFolder = tempDirPath.Path.Combine("Child Folder");
        fs.CreateDirectory(childFolder);
        var childFileB = childFolder.Combine("File B");
        fs.CreateFile(childFileB);


        await fs.WriteAllTextAsync(childFileA, "File A");
        await fs.WriteAllTextAsync(childFileB, "File B");

        fs.DirectoryExists(tempDirPath).Should().BeTrue();
        fs.FileExists(childFileA).Should().BeTrue();
        fs.FileExists(childFileB).Should().BeTrue();

        await tempDirPath.DisposeAsync();

        fs.FileExists(childFileA).Should().BeFalse();
        fs.FileExists(childFileB).Should().BeFalse();
        fs.DirectoryExists(childFolder).Should().BeFalse();
        fs.DirectoryExists(tempDirPath).Should().BeFalse();
    }
}
