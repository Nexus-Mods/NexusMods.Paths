using AutoFixture;
using JetBrains.Annotations;

namespace NexusMods.Paths.TestingHelpers;

/// <summary>
/// Extension functions to add customization to <see cref="Fixture"/>.
/// </summary>
[PublicAPI]
public static class Customizations
{
    /// <summary>
    /// Registers factories for the following types:
    /// <list type="bullet">
    ///     <item><see cref="IFileSystem"/></item>
    ///     <item><see cref="InMemoryFileSystem"/></item>
    ///     <item><see cref="AbsolutePath"/></item>
    ///     <item><see cref="RelativePath"/></item>
    ///     <item><see cref="TemporaryFileManager"/></item>
    /// </list>
    /// </summary>
    /// <param name="fixture">The provided <see cref="Fixture"/> to use.</param>
    /// <param name="useSharedFileSystem">Use a shared file system for the entire fixture.</param>
    public static void AddFileSystemCustomizations(this Fixture fixture, bool useSharedFileSystem = true)
    {
        var sharedFileSystem = useSharedFileSystem ? new InMemoryFileSystem() : null;

        fixture.Customize<InMemoryFileSystem>(composer =>
            composer.FromFactory(() => sharedFileSystem ?? new InMemoryFileSystem()));

        fixture.Customize<IFileSystem>(composer =>
            composer.FromFactory(() => sharedFileSystem ?? new InMemoryFileSystem()));

        fixture.Customize<AbsolutePath>(composer =>
            composer.FromFactory<IFileSystem, string>((fs, path) =>
            {
                var fullPath = OSInformation.Shared.IsWindows
                    ? $"C:/{path}"
                    : $"/{path}";
                return fs.FromUnsanitizedFullPath(fullPath);
            }));

        fixture.Customize<RelativePath>(composer => composer.FromFactory<string>(path => path));
        fixture.Customize<TemporaryFileManager>(composer => composer.FromFactory<IFileSystem>(fs => new TemporaryFileManager(fs)));
    }
}
