using JetBrains.Annotations;
using NexusMods.Archives.Nx.Enums;
using NexusMods.Archives.Nx.Packing;
using NexusMods.Archives.Nx.Structs;
using NexusMods.Paths.Extensions.Nx.FileProviders;
namespace NexusMods.Paths.Extensions.Nx.Extensions;

/// <summary>
/// Extension methods for NxPackerBuilder to integrate AbsolutePath-based APIs.
/// </summary>
[PublicAPI]
public static class NxPackerBuilderExtensions
{
    /// <summary>
    /// Adds a file to be packed using an <see cref="AbsolutePath"/>.
    /// </summary>
    /// <param name="builder">The <see cref="NxPackerBuilder"/> instance.</param>
    /// <param name="absolutePath">The <see cref="AbsolutePath"/> of the file to add.</param>
    /// <param name="options">The options for adding the file.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static NxPackerBuilder AddFile(this NxPackerBuilder builder, AbsolutePath absolutePath, AddFileParams options)
    {
        var packerFile = new PackerFile
        {
            FileDataProvider = new FromAbsolutePathProvider
            {
                FilePath = absolutePath
            },
            RelativePath = options.RelativePath,
            FileSize = (long)absolutePath.FileInfo.Size.Value,
            SolidType = options.SolidType,
            CompressionPreference = options.CompressionPreference
        };

        return builder.AddPackerFile(packerFile);
    }

    /// <summary>
    /// Sets the output (archive) to an <see cref="AbsolutePath"/>.
    /// </summary>
    /// <param name="builder">The <see cref="NxPackerBuilder"/> instance.</param>
    /// <param name="outputPath">The <see cref="AbsolutePath"/> where the packed archive will be saved.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static NxPackerBuilder WithOutput(this NxPackerBuilder builder, AbsolutePath outputPath)
    {
        var stream = outputPath.FileSystem.CreateFile(outputPath);
        return builder.WithOutput(stream);
    }

    /// <summary>
    /// Adds all files under a given folder to the output using <see cref="AbsolutePath"/>.
    /// </summary>
    /// <param name="builder">The <see cref="NxPackerBuilder"/> instance.</param>
    /// <param name="folderPath">The <see cref="AbsolutePath"/> of the folder to add items from.</param>
    /// <param name="solidType">The solid type preference for the files.</param>
    /// <param name="compressionPreference">The compression preference for the files.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static NxPackerBuilder AddFolder(this NxPackerBuilder builder,
        AbsolutePath folderPath,
        SolidPreference solidType = SolidPreference.Default,
        CompressionPreference compressionPreference = CompressionPreference.NoPreference)
    {
        foreach (var file in folderPath.EnumerateFiles())
        {
            var options = new AddFileParams
            {
                RelativePath = file.RelativeTo(folderPath).ToString(),
                SolidType = solidType,
                CompressionPreference = compressionPreference
            };

            builder.AddFile(file, options);
        }
        return builder;
    }
}
