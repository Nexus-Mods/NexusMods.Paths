using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveAFileOrDirectoryTests
{
    [Fact]
    public void CountFiles_ShouldReturnTotalFilesCount()
    {
        // Arrange
        var leaf1 = new TestTree(Array.Empty<ChildBox<TestTree>>(), true);
        var leaf2 = new TestTree(Array.Empty<ChildBox<TestTree>>(), true);
        var directory = new TestTree(new ChildBox<TestTree>[] { leaf1, leaf2 }, false);
        var root = new TestTree(new ChildBox<TestTree>[] { directory }, false);

        // Act
        var fileCount = root.CountFiles();

        // Assert
        fileCount.Should().Be(2);
    }

    [Fact]
    public void CountDirectories_ShouldReturnTotalDirectoriesCount()
    {
        // Arrange
        var leaf1 = new TestTree(Array.Empty<ChildBox<TestTree>>(), true);
        var directory = new TestTree(new ChildBox<TestTree>[] { leaf1 }, false);
        var root = new TestTree(new ChildBox<TestTree>[] { directory }, false);

        // Act
        var directoryCount = root.CountDirectories();

        // Assert
        directoryCount.Should().Be(1); // Only the root's child is a directory
    }

    public struct TestTree : IHaveBoxedChildren<TestTree>, IHaveAFileOrDirectory
    {
        public ChildBox<TestTree>[] Children { get; }
        public bool IsFile { get; }
        public bool IsDirectory => !IsFile;

        public TestTree(ChildBox<TestTree>[] children, bool isFile)
        {
            Children = children;
            IsFile = isFile;
        }
    }
}
