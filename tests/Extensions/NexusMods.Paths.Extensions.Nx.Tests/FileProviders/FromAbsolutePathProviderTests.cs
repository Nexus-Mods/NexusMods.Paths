using FluentAssertions;
using NexusMods.Paths.Extensions.Nx.FileProviders;
using Xunit;
namespace NexusMods.Paths.Extensions.Nx.Tests.FileProviders;

public class FromAbsolutePathProviderTests
{
   public static IEnumerable<object[]> FileSystemTypes()
    {
        yield return new object[] { FileSystem.Shared };
        yield return new object[] { new InMemoryFileSystem() };
    }

    [Theory]
    [MemberData(nameof(FileSystemTypes))]
    public async Task GetFileData_ReturnsCorrectData(IFileSystem fileSystem)
    {
        // Arrange
        var path = await CreateTestFile(fileSystem, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        var provider = new FromAbsolutePathProvider { FilePath = path };

        // Act
        using (var fileData = provider.GetFileData(2, 3))
        {
            // Assert
            fileData.DataLength.Should().Be(3ul);
            unsafe
            {
                fileData.Data[0].Should().Be(2);
                fileData.Data[1].Should().Be(3);
                fileData.Data[2].Should().Be(4);
            }
        }

        // Cleanup
        CleanupTestFile(fileSystem, path);
    }

    [Theory]
    [MemberData(nameof(FileSystemTypes))]
    public async Task GetFileData_HandlesOverflow(IFileSystem fileSystem)
    {
        // Arrange
        var path = await CreateTestFile(fileSystem, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        var provider = new FromAbsolutePathProvider { FilePath = path };

        // Act
        using (var fileData = provider.GetFileData(8, 5))
        {
            // Assert
            fileData.DataLength.Should().Be(2ul);
            unsafe
            {
                fileData.Data[0].Should().Be(8);
                fileData.Data[1].Should().Be(9);
            }
        }

        // Cleanup
        CleanupTestFile(fileSystem, path);
    }

    [Theory]
    [MemberData(nameof(FileSystemTypes))]
    public async Task GetFileData_HandlesEmptyFile(IFileSystem fileSystem)
    {
        // Arrange
        var path = await CreateTestFile(fileSystem, Array.Empty<byte>());
        var provider = new FromAbsolutePathProvider { FilePath = path };

        // Act
        using (var fileData = provider.GetFileData(0, 5))
        {
            // Assert
            fileData.DataLength.Should().Be(0ul);
        }

        // Cleanup
        CleanupTestFile(fileSystem, path);
    }

    [Theory]
    [MemberData(nameof(FileSystemTypes))]
    public async Task GetFileData_HandlesOutOfBoundsOverflow(IFileSystem fileSystem)
    {
        // Arrange
        var path = await CreateTestFile(fileSystem, new byte[] { 0, 1, 2, 3, 4 });
        var provider = new FromAbsolutePathProvider { FilePath = path };

        // Act
        using (var fileData = provider.GetFileData(10, 5))
        {
            // Assert
            fileData.DataLength.Should().Be(0ul);
        }


        // Cleanup
        CleanupTestFile(fileSystem, path);
    }

    private static async Task<AbsolutePath> CreateTestFile(IFileSystem fileSystem, byte[] testData)
    {
        var path = fileSystem.GetKnownPath(KnownPath.TempDirectory).Combine(Guid.NewGuid().ToString());
        await fileSystem.WriteAllBytesAsync(path, testData);
        return path;
    }

    private static void CleanupTestFile(IFileSystem fileSystem, AbsolutePath path)
    {
        if (fileSystem.FileExists(path))
            fileSystem.DeleteFile(path);
    }
}
