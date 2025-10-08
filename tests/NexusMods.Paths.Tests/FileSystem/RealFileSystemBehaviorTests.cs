namespace NexusMods.Paths.Tests.FileSystem;

public sealed class RealFileSystemBehaviorTests : FileSystemBehaviorTestsBase
{
    protected override Ctx CreateContextWithInitialFile(string relativePath = "a/file.txt", string content = "payload")
    {
        var fs = (NexusMods.Paths.FileSystem)NexusMods.Paths.FileSystem.Shared;
        var tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var root = fs.FromUnsanitizedFullPath(tempRoot);
        var abs = root / RelativePath.FromUnsanitizedInput(relativePath);
        fs.CreateDirectory(abs.Parent);
        fs.WriteAllText(abs, content);
        return new Ctx(fs, root, abs, () => { try { Directory.Delete(tempRoot, true); } catch { } });
    }
}