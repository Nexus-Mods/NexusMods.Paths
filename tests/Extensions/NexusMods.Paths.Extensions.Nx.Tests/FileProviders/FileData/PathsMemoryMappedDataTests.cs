using FluentAssertions;
using NexusMods.Paths.Extensions.Nx.FileProviders.FileData;
using Xunit;
namespace NexusMods.Paths.Extensions.Nx.Tests.FileProviders.FileData;

public unsafe class PathsMemoryMappedDataTests
{
    [Fact]
    public void Constructor_RespectsStartOffsetAndLength()
    {
        // Arrange
        var testData = new byte[] { 1, 2, 3, 4, 5 };
        fixed (byte* testDataPtr = &testData[0])
        {
            var testHandle = new MemoryMappedFileHandle(testDataPtr, (nuint)testData.Length, null);

            // Act
            var fileData = new PathsMemoryMappedFileData(testHandle, 1, 3, false);

            // Assert
            fileData.DataLength.Should().Be(3ul);
            fileData.Data[0].Should().Be(2);
            fileData.Data[1].Should().Be(3);
            fileData.Data[2].Should().Be(4);
        }
    }

    [Fact]
    public void Constructor_HandlesOverflow()
    {
        // Arrange
        var testData = new byte[] { 1, 2, 3, 4, 5 };
        fixed (byte* testDataPtr = &testData[0])
        {
            var testHandle = new MemoryMappedFileHandle(testDataPtr, (nuint)testData.Length, null);

            // Act
            var fileData = new PathsMemoryMappedFileData(testHandle, 3, 10, false);

            // Assert
            fileData.DataLength.Should().Be(2ul);
            fileData.Data[0].Should().Be(4);
            fileData.Data[1].Should().Be(5);
        }
    }
}
