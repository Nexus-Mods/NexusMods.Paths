using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveFileOrDirectoryTests
{
    [Fact]
    public void CountFiles_ShouldReturnTotalFilesCount()
    {
        // Arrange
        var leaf1 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>>(), true);
        var leaf2 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>>(), true);
        var directory = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>>
        {
            [1] = leaf1,
            [2] = leaf2
        }, false);
        var root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { [0] = directory }, false);

        // Act
        var fileCount = root.CountFiles<TestTree, int>();

        // Assert
        fileCount.Should().Be(2);
    }

    [Fact]
    public void CountDirectories_ShouldReturnTotalDirectoriesCount()
    {
        // Arrange
        var leaf1 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>>(), true);
        var directory = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { [1] = leaf1 }, false);
        var root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { [0] = directory }, false);

        // Act
        var directoryCount = root.CountDirectories<TestTree, int>();

        // Assert
        directoryCount.Should().Be(1); // Only the root's child is a directory
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveAFileOrDirectory
    {
        public Dictionary<int, ChildWithKeyBox<int, TestTree>> Children { get; }
        public bool IsFile { get; }
        public bool IsDirectory => !IsFile;

        public TestTree(Dictionary<int, ChildWithKeyBox<int, TestTree>> children, bool isFile)
        {
            Children = children;
            IsFile = isFile;
        }
    }
}
