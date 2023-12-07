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
        var root =  TestTree.Create(false, new Dict { [0] = directory });

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
        var root = TestTree.Create(false, new Dict { [0] = directory1, [1] = directory2 });

        // Act
        var directoryCount = root.CountDirectories();

        // Assert
        directoryCount.Should().Be(2);
    }

    [Fact]
    public void EnumerateFilesBfs_ShouldEnumerateAllFilesInBreadthFirstManner()
    {
        // Arrange
        var leaf1 = TestTree.Create(true);
        var leaf2 = TestTree.Create(true);
        var directory = TestTree.Create(false, new Dict { [1] = leaf1, [2] = leaf2 });
        var root = TestTree.Create(false, new Dict { [0] = directory });

        // Act
        var enumeratedFiles = root.EnumerateFilesBfs().ToList();

        // Assert
        enumeratedFiles.Count.Should().Be(2);
        enumeratedFiles.Select(kvp => kvp.Value).Should().Contain(leaf1);
        enumeratedFiles.Select(kvp => kvp.Value).Should().Contain(leaf2);
    }

    [Fact]
    public void EnumerateDirectoriesBfs_ShouldEnumerateAllDirectoriesInBreadthFirstManner()
    {
        // Arrange
        var leaf1 = TestTree.Create(true);
        var directory1 = TestTree.Create(false, new Dict { [1] = leaf1 });
        var directory2 = TestTree.Create(false);
        var root = TestTree.Create(false, new Dict { [0] = directory1, [1] = directory2 });

        // Act
        var enumeratedDirectories = root.EnumerateDirectoriesBfs().ToList();

        // Assert
        enumeratedDirectories.Count.Should().Be(2);
        enumeratedDirectories.Select(kvp => kvp.Value).Should().Contain(directory1);
        enumeratedDirectories.Select(kvp => kvp.Value).Should().Contain(directory2);
    }

    [Fact]
    public void EnumerateFilesDfs_ShouldEnumerateAllFilesInDepthFirstManner()
    {
        // Arrange
        var deepLeaf = TestTree.Create(true);
        var shallowLeaf = TestTree.Create(true);
        var subDirectory = TestTree.Create(false, new Dict { [0] = deepLeaf });
        var directory = TestTree.Create(false, new Dict { [0] = subDirectory, [1] = shallowLeaf });
        var root = TestTree.Create(false, new Dict { [0] = directory });

        // Act
        var enumeratedFiles = root.EnumerateFilesDfs().ToList();

        // Assert
        enumeratedFiles.Count.Should().Be(2);
        enumeratedFiles.Select(kvp => kvp.Value).Should().Contain(deepLeaf);
        enumeratedFiles.Select(kvp => kvp.Value).Should().Contain(shallowLeaf);
    }

    [Fact]
    public void EnumerateDirectoriesDfs_ShouldEnumerateAllDirectoriesInDepthFirstManner()
    {
        // Arrange
        var leafInDeepDirectory = TestTree.Create(true);
        var deeperSubDirectory = TestTree.Create(false, new Dict { [0] = leafInDeepDirectory });
        var shallowDirectory = TestTree.Create(false);
        var deepDirectory = TestTree.Create(false, new Dict { [0] = deeperSubDirectory });
        var root = TestTree.Create(false, new Dict { [0] = deepDirectory, [1] = shallowDirectory });

        // Act
        var enumeratedDirectories = root.EnumerateDirectoriesDfs().ToList();

        // Assert
        enumeratedDirectories.Count.Should().Be(3);
        enumeratedDirectories.Select(kvp => kvp.Value).Should().Contain(deepDirectory);
        enumeratedDirectories.Select(kvp => kvp.Value).Should().Contain(deeperSubDirectory);
        enumeratedDirectories.Select(kvp => kvp.Value).Should().Contain(shallowDirectory);
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
