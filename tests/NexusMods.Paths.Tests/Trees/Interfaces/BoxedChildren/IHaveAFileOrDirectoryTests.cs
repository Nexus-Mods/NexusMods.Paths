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
        var leaf1 = TestTree.Create(true);
        var leaf2 = TestTree.Create(true);
        var directory = TestTree.Create(false, new[] { leaf1, leaf2 });
        var root = TestTree.Create(false, new[] { directory });

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
        var directory = TestTree.Create(false, leaf1);
        var root = TestTree.Create(false, directory);

        // Act
        var directoryCount = root.CountDirectories();

        // Assert
        directoryCount.Should().Be(1); // Only the root's child is a directory
    }

    [Fact]
    public void EnumerateFilesBfs_ShouldEnumerateAllFilesInBreadthFirstManner()
    {
        // Arrange
        var leaf1 = TestTree.Create(true);
        var leaf2 = TestTree.Create(true);
        var directory = TestTree.Create(false, new[] { leaf1, leaf2 });
        var root = TestTree.Create(false, new[] { directory });

        // Act
        var enumeratedFiles = root.EnumerateFilesBfs().ToList();

        // Assert
        enumeratedFiles.Count.Should().Be(2);
        enumeratedFiles[0].Should().Be(leaf1);
        enumeratedFiles[1].Should().Be(leaf2);
    }

    [Fact]
    public void EnumerateDirectoriesBfs_ShouldEnumerateAllDirectoriesInBreadthFirstManner()
    {
        // Arrange
        var leaf1 = TestTree.Create(true);
        var directory1 = TestTree.Create(false, leaf1);
        var directory2 = TestTree.Create(false);
        var root = TestTree.Create(false, new[] { directory1, directory2 });

        // Act
        var enumeratedDirectories = root.EnumerateDirectoriesBfs().ToList();

        // Assert
        enumeratedDirectories.Count.Should().Be(2);
        enumeratedDirectories[0].Should().Be(directory1);
        enumeratedDirectories[1].Should().Be(directory2);
    }

    [Fact]
    public void EnumerateFilesDfs_ShouldEnumerateAllFilesInDepthFirstManner()
    {
        // Arrange
        var deepLeaf = TestTree.Create(true); // Deeper file
        var shallowLeaf = TestTree.Create(true); // Shallow file
        var subDirectory = TestTree.Create(false, new[] { deepLeaf });
        var directory = TestTree.Create(false, new[] { subDirectory, shallowLeaf });
        var root = TestTree.Create(false, new[] { directory });

        // Act
        var enumeratedFiles = root.EnumerateFilesDfs().ToList();

        // Assert
        enumeratedFiles.Count.Should().Be(2);
        enumeratedFiles[0].Should().Be(deepLeaf); // Deeper file should come first
        enumeratedFiles[1].Should().Be(shallowLeaf); // Followed by the shallow file
    }

    [Fact]
    public void EnumerateDirectoriesDfs_ShouldEnumerateAllDirectoriesInDepthFirstManner()
    {
        // Arrange
        var leafInDeepDirectory = TestTree.Create(true);
        var deeperSubDirectory = TestTree.Create(false, new[] { leafInDeepDirectory }); // Nested deeper
        var shallowDirectory = TestTree.Create(false); // Shallow directory
        var deepDirectory = TestTree.Create(false, new[] { deeperSubDirectory }); // Contains deeperSubDirectory
        var root = TestTree.Create(false, new[] { deepDirectory, shallowDirectory });

        // Act
        var enumeratedDirectories = root.EnumerateDirectoriesDfs().ToList();

        // Assert
        enumeratedDirectories.Count.Should().Be(3);
        enumeratedDirectories[0].Should().Be(deepDirectory); // First, deepDirectory should come
        enumeratedDirectories[1].Should().Be(deeperSubDirectory); // Then, deeperSubDirectory inside deepDirectory
        enumeratedDirectories[2].Should().Be(shallowDirectory); // Finally, the shallowDirectory
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveAFileOrDirectory
    {
        public Box<TestTree>[] Children { get; private init; }
        public bool IsFile { get; private init; }
        public bool IsDirectory => !IsFile;

        public static Box<TestTree> Create(bool isFile, Box<TestTree>[]? children)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = children ?? Array.Empty<Box<TestTree>>(),
                IsFile = isFile
            };
        }

        public static Box<TestTree> Create(bool isFile, Box<TestTree> child)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = new[] { new Box<TestTree> { Item = child } },
                IsFile = isFile
            };
        }

        public static Box<TestTree> Create(bool isFile)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = Array.Empty<Box<TestTree>>(),
                IsFile = isFile
            };
        }
    }
}
