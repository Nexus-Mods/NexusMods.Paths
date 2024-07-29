using FluentAssertions;
using NexusMods.Archives.Nx.FileProviders.FileData;
using NexusMods.Archives.Nx.Headers.Managed;
using NexusMods.Paths.Extensions.Nx.FileProviders;
using Xunit;
namespace NexusMods.Paths.Extensions.Nx.Tests.FileProviders;

public class OutputAbsolutePathProviderTests
{
    public static IEnumerable<object[]> FileSystemTypes()
    {
        yield return new object[] { FileSystem.Shared };
        yield return new object[] { new InMemoryFileSystem() };
    }

    [Theory]
    [MemberData(nameof(FileSystemTypes))]
    public void GetFileData_ReturnsCorrectData(IFileSystem fileSystem)
    {
        // Arrange
        var path = CreateTestPath(fileSystem);
        var entry = new FileEntry { DecompressedSize = 100 };
        var provider = new OutputAbsolutePathProvider(path, "relative/path.txt", entry);

        // Act
        var fileData = provider.GetFileData(10, 5);

        // Assert
        fileData.DataLength.Should().Be(5ul);

        // Cleanup
        CleanupTestFile(fileSystem, path);
    }

    [Theory]
    [MemberData(nameof(FileSystemTypes))]
    public void GetFileData_HandlesEmptyFile(IFileSystem fileSystem)
    {
        // Arrange
        var path = CreateTestPath(fileSystem);
        var entry = new FileEntry { DecompressedSize = 0 };
        var provider = new OutputAbsolutePathProvider(path, "relative/path.txt", entry);

        // Act
        var fileData = provider.GetFileData(0, 5);

        // Assert
        fileData.DataLength.Should().Be(0ul);
        Assert.IsType<ArrayFileData>(fileData);

        // Cleanup
        CleanupTestFile(fileSystem, path);
    }

    [Theory]
    [MemberData(nameof(FileSystemTypes))]
    public void Constructor_CreatesFile(IFileSystem fileSystem)
    {
        // Arrange
        var path = CreateTestPath(fileSystem);
        var entry = new FileEntry { DecompressedSize = 100 };

        // Act
        _ = new OutputAbsolutePathProvider(path, "relative/path.txt", entry);

        // Assert
        fileSystem.FileExists(path).Should().BeTrue();

        // Cleanup
        CleanupTestFile(fileSystem, path);
    }

    private static AbsolutePath CreateTestPath(IFileSystem fileSystem)
    {
        return fileSystem.GetKnownPath(KnownPath.TempDirectory).Combine(Guid.NewGuid().ToString());
    }

    private static void CleanupTestFile(IFileSystem fileSystem, AbsolutePath path)
    {
        if (fileSystem.FileExists(path))
        {
            fileSystem.DeleteFile(path);
        }
    }
}
