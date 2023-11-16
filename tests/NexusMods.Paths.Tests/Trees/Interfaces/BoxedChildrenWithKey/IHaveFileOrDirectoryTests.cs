using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;
using Dict = System.Collections.Generic.Dictionary<int, NexusMods.Paths.Trees.KeyedBox<int, NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey.IHaveFileOrDirectoryTests.TestTree>>;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveFileOrDirectoryTests
{
    [Fact]
    public void CountFiles_ShouldReturnTotalFilesCount()
    {
        // Arrange
        var leaf1 = TestTree.Create(true);
        var leaf2 = TestTree.Create(true);
        var directory = TestTree.Create(false, new Dict
        {
            [1] = leaf1,
            [2] = leaf2
        });
        KeyedBox<int, TestTree> root =  TestTree.Create(false, new Dict { [0] = directory });

        // Act
        var fileCount = root.CountFiles();

        // Assert
        fileCount.Should().Be(2);
    }

    [Fact]
    public void CountDirectories_ShouldReturnTotalDirectoriesCount()
    {
        // Arrange
        var leaf1 = TestTree.Create(true);
        var leaf2 = TestTree.Create(true);
        var directory1 = TestTree.Create(false, new Dict { [1] = leaf1 });
        var directory2 = TestTree.Create(false, new Dict { [2] = leaf2 });
        KeyedBox<int, TestTree> root = TestTree.Create(false, new Dict { [0] = directory1, [1] = directory2 });

        // Act
        var directoryCount = root.CountDirectories();

        // Assert
        directoryCount.Should().Be(2);
    }

    internal struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveAFileOrDirectory
    {
        public Dict Children { get; private init; }
        public bool IsFile { get; private init; }

        public static KeyedBox<int, TestTree> Create(bool isFile, Dict? children = null)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Children = children ?? new Dict(),
                IsFile = isFile
            };
        }
    }
}
