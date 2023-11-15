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
