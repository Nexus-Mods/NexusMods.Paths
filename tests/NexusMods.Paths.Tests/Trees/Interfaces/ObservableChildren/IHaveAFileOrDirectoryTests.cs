using System.Collections.ObjectModel;
using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.ObservableChildren;

// ReSharper disable once InconsistentNaming
public class IHaveAFileOrDirectoryTests
{
    [Fact]
    public void CountFiles_ShouldReturnTotalFilesCount()
    {
        // Arrange
        var leaf1 = TestTree.Create(true);
        var leaf2 = TestTree.Create(true);
        var directory = TestTree.Create(false, new ObservableCollection<Box<TestTree>> { leaf1, leaf2 });
        var root = TestTree.Create(false, new ObservableCollection<Box<TestTree>> { directory });

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

    private struct TestTree : IHaveObservableChildren<TestTree>, IHaveAFileOrDirectory
    {
        public ObservableCollection<Box<TestTree>> Children { get; private init; }
        public bool IsFile { get; private init; }
        public bool IsDirectory => !IsFile;

        public static Box<TestTree> Create(bool isFile, ObservableCollection<Box<TestTree>>? children)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = children ?? new ObservableCollection<Box<TestTree>>(),
                IsFile = isFile
            };
        }

        public static Box<TestTree> Create(bool isFile, Box<TestTree> child)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = new ObservableCollection<Box<TestTree>> { new() { Item = child } },
                IsFile = isFile
            };
        }

        public static Box<TestTree> Create(bool isFile)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = new ObservableCollection<Box<TestTree>>(),
                IsFile = isFile
            };
        }
    }
}
