using System.Collections.ObjectModel;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.ObservableChildren;

// ReSharper disable once InconsistentNaming
public class IHaveAFileOrDirectoryTests
{
    [Fact]
    public void CountFiles_ShouldReturnTotalFilesCount()
    {
        // Arrange
        var leaf1 = new TestTree(new(), true);
        var leaf2 = new TestTree(new(), true);
        var directory = new TestTree(new ObservableCollection<ChildBox<TestTree>> { leaf1, leaf2 }, false);
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { directory }, false);

        // Act
        var fileCount = root.CountFiles();

        // Assert
        fileCount.Should().Be(2);
    }

    [Fact]
    public void CountDirectories_ShouldReturnTotalDirectoriesCount()
    {
        // Arrange
        var leaf1 = new TestTree(new(), true);
        var directory = new TestTree(new ObservableCollection<ChildBox<TestTree>> { leaf1 }, false);
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { directory }, false);

        // Act
        var directoryCount = root.CountDirectories();

        // Assert
        directoryCount.Should().Be(1); // Only the root's child is a directory
    }

    private struct TestTree : IHaveObservableChildren<TestTree>, IHaveAFileOrDirectory
    {
        public ObservableCollection<ChildBox<TestTree>> Children { get; }
        public bool IsFile { get; }
        public bool IsDirectory => !IsFile;

        public TestTree(ObservableCollection<ChildBox<TestTree>> children, bool isFile)
        {
            Children = children;
            IsFile = isFile;
        }
    }
}
