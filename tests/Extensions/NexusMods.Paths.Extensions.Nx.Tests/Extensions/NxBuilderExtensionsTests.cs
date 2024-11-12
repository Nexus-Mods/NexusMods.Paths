using FluentAssertions;
using NexusMods.Archives.Nx.Packing;
using NexusMods.Paths.Extensions.Nx.Extensions;
using NexusMods.Paths.TestingHelpers;
using Xunit;
namespace NexusMods.Paths.Extensions.Nx.Tests.Extensions;

/// <summary>
///     These tests are more of a 'sanity' check to ensure that our extensions
///     integrate correctly with Nx. They don't have much substance, other than
///     confirming that stuff 'just works'.
/// </summary>
public class NxBuilderExtensionsTests
{
    [Theory, AutoFileSystem]
    public async Task NxPackerBuilder_CanAddFolderFromIFileSystem_AndExtractToIFileSystem(InMemoryFileSystem fs, AbsolutePath folderPath)
    {
        // Arrange
        var file1 = folderPath / "file1.txt";
        var file2 = folderPath / "subfolder/file2.txt";
        await fs.WriteAllTextAsync(file1, "Content 1");
        await fs.WriteAllTextAsync(file2, "Content 2");

        var builder = new NxPackerBuilder();
        var outputPath = folderPath.Parent / "output.nx";

        // Act
        builder.AddFolder(folderPath)
            .WithOutput(outputPath)
            .Build();

        // Assert
        fs.FileExists(outputPath).Should().BeTrue();
        var unpacker = NxUnpackerBuilderExtensions.FromFile(outputPath);
        var entries = unpacker.GetPathedFileEntries();
        entries.Should().HaveCount(2);
        entries.Should().Contain(e => e.FilePath == "file1.txt");
        entries.Should().Contain(e => e.FilePath == "subfolder/file2.txt");
        
        // Verify we can extract all files
        var extractFolder = folderPath.Parent / "extracted";
        unpacker.AddAllFilesWithFileSystemOutput(extractFolder).Extract();

        var extractedFile1 = extractFolder / "file1.txt";
        var extractedFile2 = extractFolder / "subfolder/file2.txt";

        fs.FileExists(extractedFile1).Should().BeTrue();
        fs.FileExists(extractedFile2).Should().BeTrue();

        (await fs.ReadAllTextAsync(extractedFile1)).Should().Be("Content 1");
        (await fs.ReadAllTextAsync(extractedFile2)).Should().Be("Content 2");
        
        // Verify we can extract a single file.
        unpacker = NxUnpackerBuilderExtensions.FromFile(outputPath);
        var extractedFile1Copy = extractFolder / "file1-copy.txt";
        var file1Entry = entries.First(x => x.FilePath == "file1.txt");
        unpacker.AddFileWithFileSystemOutput(file1Entry, extractedFile1Copy).Extract();
        (await fs.ReadAllTextAsync(extractedFile1Copy)).Should().Be("Content 1");
    }
}
