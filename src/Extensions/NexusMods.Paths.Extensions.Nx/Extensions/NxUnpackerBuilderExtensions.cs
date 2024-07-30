using JetBrains.Annotations;
using NexusMods.Archives.Nx.Packing;
using NexusMods.Archives.Nx.Packing.Unpack;
using NexusMods.Paths.Extensions.Nx.FileProviders;
namespace NexusMods.Paths.Extensions.Nx.Extensions;

/// <summary>
/// Extension methods for <see cref="NxUnpackerBuilder"/> to integrate <see cref="AbsolutePath"/>-based APIs.
/// </summary>
[PublicAPI]
public static class NxUnpackerBuilderExtensions
{
    /// <summary>
    /// Creates an <see cref="NxUnpackerBuilder"/> instance using an AbsolutePath.
    /// </summary>
    /// <param name="archivePath">The <see cref="AbsolutePath"/> of the .nx archive file.</param>
    /// <param name="hasLotsOfFiles">Hint whether the archive contains lots of files (100+).</param>
    /// <returns>A new instance of <see cref="NxUnpackerBuilder"/>.</returns>
    public static NxUnpackerBuilder FromFile(AbsolutePath archivePath, bool hasLotsOfFiles = false)
    {
        return new NxUnpackerBuilder(new FromAbsolutePathProvider
        {
            FilePath = archivePath
        }, hasLotsOfFiles);
    }

    /// <summary>
    /// Extracts files to a specified directory using <see cref="AbsolutePath"/>.
    /// </summary>
    /// <param name="builder">The <see cref="NxUnpackerBuilder"/> instance.</param>
    /// <param name="outputDirectory">The <see cref="AbsolutePath"/> of the directory to extract files to.</param>
    /// <param name="entries">The file entries to extract.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static NxUnpackerBuilder AddFilesWithFileSystemOutput(this NxUnpackerBuilder builder, AbsolutePath outputDirectory, PathedFileEntry[] entries)
    {
        var outputProviders = new OutputAbsolutePathProvider[entries.Length];
        for (var x = 0; x < entries.Length; x++)
        {
            var entry = entries[x];
            var outputPath = outputDirectory.Combine(entry.FilePath);
            outputProviders[x] = new OutputAbsolutePathProvider(outputPath, entry.FilePath, entry.Entry);
        }
        builder.Outputs.AddRange(outputProviders);
        return builder;
    }

    /// <summary>
    /// Extracts all files to a specified directory using AbsolutePath.
    /// </summary>
    /// <param name="builder">The <see cref="NxUnpackerBuilder"/> instance.</param>
    /// <param name="outputDirectory">The <see cref="AbsolutePath"/> of the directory to extract files to.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static NxUnpackerBuilder AddAllFilesWithFileSystemOutput(this NxUnpackerBuilder builder, AbsolutePath outputDirectory)
    {
        return builder.AddFilesWithFileSystemOutput(outputDirectory, builder.GetPathedFileEntries());
    }

    /// <summary>
    /// Extracts a single file to a specified path using <see cref="AbsolutePath"/>.
    /// </summary>
    /// <param name="builder">The <see cref="NxUnpackerBuilder"/> instance.</param>
    /// <param name="entry">The file entry to extract.</param>
    /// <param name="outputPath">The <see cref="AbsolutePath"/> where the file should be extracted.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static NxUnpackerBuilder AddFileWithFileSystemOutput(this NxUnpackerBuilder builder, PathedFileEntry entry, AbsolutePath outputPath)
    {
        // Output provider disposed during extract.
        builder.Outputs.Add(new OutputAbsolutePathProvider(outputPath, entry.FilePath, entry.Entry));
        return builder;
    }
}
