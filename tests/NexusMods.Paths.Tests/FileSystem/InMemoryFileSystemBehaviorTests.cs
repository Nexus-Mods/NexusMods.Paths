namespace NexusMods.Paths.Tests.FileSystem;

public sealed class InMemoryFileSystemBehaviorTests : FileSystemBehaviorTestsBase
{
    protected override Ctx CreateContextWithInitialFile(string relativePath = "a/file.txt", string content = "payload")
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix);
        var root = fs.FromUnsanitizedFullPath("/mnt");
        var abs = root / RelativePath.FromUnsanitizedInput(relativePath);
        fs.CreateDirectory(abs.Parent);
        fs.WriteAllText(abs, content);
        return new Ctx(fs, root, abs, () => { });
    }
}
