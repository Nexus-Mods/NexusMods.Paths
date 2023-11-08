using NexusMods.Paths.Trees.Traits;
using Dict = System.Collections.Generic.Dictionary<int, NexusMods.Paths.Trees.Traits.ChildWithKeyBox<int, NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey.IHaveFileOrDirectoryTests.TestTree>>;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveFileOrDirectoryTests
{
    [Fact]
    public void CountFiles_ShouldReturnTotalFilesCount()
    {
        // Arrange
        var leaf1 = new TestTree(new Dict(), true);
        var leaf2 = new TestTree(new Dict(), true);
        var directory = new TestTree(new Dict
        {
            [1] = leaf1,
            [2] = leaf2
        }, false);
        ChildWithKeyBox<int, TestTree> root =  new TestTree(new Dict { [0] = directory }, false);

        // Act
        var fileCount = root.CountFiles();

        // Assert
        fileCount.Should().Be(2);
    }

    [Fact]
    public void CountDirectories_ShouldReturnTotalDirectoriesCount()
    {
        // Arrange
        var leaf1 = new TestTree(new Dict(), true);
        var leaf2 = new TestTree(new Dict(), true);
        var directory1 = new TestTree(new Dict { [1] = leaf1 }, false);
        var directory2 = new TestTree(new Dict { [2] = leaf2 }, false);
        ChildWithKeyBox<int, TestTree> root = new TestTree(new Dict { [0] = directory1, [1] = directory2 }, false);

        // Act
        var directoryCount = root.CountDirectories();

        // Assert
        directoryCount.Should().Be(2);
    }

    internal struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveAFileOrDirectory
    {
        public Dict Children { get; }
        public bool IsFile { get; }

        public TestTree(Dict children, bool isFile)
        {
            Children = children;
            IsFile = isFile;
        }
    }
}
