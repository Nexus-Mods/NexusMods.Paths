using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveAFileOrDirectoryTests
{
    [Fact]
    public void CountFiles_ShouldReturnTotalFilesCount()
    {
        // Arrange
        var leaf1 = new TestTree(Array.Empty<Box<TestTree>>(), true);
        var leaf2 = new TestTree(Array.Empty<Box<TestTree>>(), true);
        var directory = new TestTree(new Box<TestTree>[] { leaf1, leaf2 }, false);
        Box<TestTree> root = new TestTree(new Box<TestTree>[] { directory }, false);

        // Act
        var fileCount = root.CountFiles();

        // Assert
        fileCount.Should().Be(2);
    }

    [Fact]
    public void CountDirectories_ShouldReturnTotalDirectoriesCount()
    {
        // Arrange
        var leaf1 = new TestTree(Array.Empty<Box<TestTree>>(), true);
        var directory = new TestTree(new Box<TestTree>[] { leaf1 }, false);
        Box<TestTree> root = new TestTree(new Box<TestTree>[] { directory }, false);

        // Act
        var directoryCount = root.CountDirectories();

        // Assert
        directoryCount.Should().Be(1); // Only the root's child is a directory
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveAFileOrDirectory
    {
        public Box<TestTree>[] Children { get; }
        public bool IsFile { get; }
        public bool IsDirectory => !IsFile;

        public TestTree(Box<TestTree>[] children, bool isFile)
        {
            Children = children;
            IsFile = isFile;
        }
    }
}
