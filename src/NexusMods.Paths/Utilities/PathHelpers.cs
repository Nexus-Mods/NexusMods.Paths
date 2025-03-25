using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reloaded.Memory.Extensions;

namespace NexusMods.Paths.Utilities;

/// <summary>
/// Helper methods for dealing with paths.
/// </summary>
[PublicAPI]
public static class PathHelpers
{
    [ExcludeFromCodeCoverage(Justification = "Impossible to test.")]
    static PathHelpers()
    {
        // NOTE: The forward slash '/' is supported on BOTH Windows and Unix-based systems.
        // On Windows: Path.DirectorySeparatorChar = '\' and Path.AltDirectorySeparatorChar = '/'
        // On Linux: Path.DirectorySeparatorChar = '/' and Path.AltDirectorySeparatorChar = '/'
        // As such, we can use the forward slash for all supported platforms.
        // See https://learn.microsoft.com/en-us/dotnet/api/system.io.path.directoryseparatorchar#remarks

        if (Path.DirectorySeparatorChar != DirectorySeparatorChar &&
            Path.AltDirectorySeparatorChar != DirectorySeparatorChar)
        {
            // This is pretty impossible to reach, since Windows, Linux, macOS and
            // other Unix-based systems have the forward slash '/' as either the main
            // directory separator or, at the very least, as the alt directory separator.
            throw new PlatformNotSupportedException(
                "The current platform doesn't support the forward slash as a directory separator!" +
                $"Supported directory separators are: '{Path.DirectorySeparatorChar}' and {Path.AltDirectorySeparatorChar}");
        }
    }

    /// <summary>
    /// Character used to separate directory levels in a path that reflects a hierarchical file system organization.
    /// </summary>
    /// <seealso cref="DirectorySeparatorString"/>
    public const char DirectorySeparatorChar = '/';

    /// <summary>
    /// <see cref="DirectorySeparatorChar"/> as a string.
    /// </summary>
    /// <seealso cref="DirectorySeparatorChar"/>
    public const string DirectorySeparatorString = "/";

    /// <summary>
    /// Character used to separate extensions from the file name.
    /// </summary>
    public const char ExtensionSeparatorChar = '.';

    /// <summary>
    /// Volume separator character on Windows.
    /// </summary>
    /// <remarks>
    /// This character is used to separate the drive character of a volume, from the rest
    /// of the path. The path "C:/" has the drive character 'C', the volume separator character
    /// ':' and finally the root directory name '/'.
    /// </remarks>
    public const char WindowsVolumeSeparatorChar = ':';

    /// <summary>
    /// Debug assert <see cref="IsSanitized"/>.
    /// </summary>
    [Conditional("DEBUG")]
    [ExcludeFromCodeCoverage(Justification = $"{nameof(IsSanitized)} is tested separately.")]
    public static void DebugAssertIsSanitized(ReadOnlySpan<char> path, IOSInformation os)
    {
        var isRelative = !IsRooted(path, os);
        Debug.Assert(IsSanitized(path, os, isRelative), $"Path is not sanitized: '{path.ToString()}'");
    }

    /// <summary>
    /// Determines whether the path is sanitized or not. Only sanitized paths should
    /// be used with <see cref="PathHelpers"/>.
    /// </summary>
    public static bool IsSanitized(ReadOnlySpan<char> path, IOSInformation os, bool isRelative)
    {
        // Empty strings are valid.
        if (path.IsEmpty) return true;

        var rootLength = GetRootLength(path, os);

        // Relative paths must not be rooted
        if (isRelative && rootLength != -1) return false;

        // Absolute paths must be rooted
        if (!isRelative && rootLength == -1) return false;

        // Paths that only contain the root directory are valid.
        if (!isRelative && rootLength == path.Length) return true;

        // Paths must be trimmed
        if (path.DangerousGetReferenceAt(path.Length - 1) == ' ') return false;

        // Paths that end with the directory separator but are not root directories are invalid.
        if (path.DangerousGetReferenceAt(path.Length - 1) == DirectorySeparatorChar) return false;

        // Paths on Windows that use backwards slashes '\' instead of forward ones are invalid.
        // ReSharper disable once RedundantNameQualifier
        if (Reloaded.Memory.Extensions.SpanExtensions.Count(path, '\\') != 0) return false;

        // Everything else is valid.
        return true;
    }

    /// <summary>
    /// Removes trailing directory separator characters from the input.
    /// </summary>
    public static ReadOnlySpan<char> RemoveTrailingDirectorySeparator(ReadOnlySpan<char> path)
    {
        return path.DangerousGetReferenceAt(path.Length - 1) == DirectorySeparatorChar
            ? path.SliceFast(0, path.Length - 1)
            : path;
    }

    /// <summary>
    /// Sanitizes the given path. Only sanitized paths should be used with
    /// <see cref="PathHelpers"/>.
    /// </summary>
    [SkipLocalsInit]
    public static string Sanitize(ReadOnlySpan<char> path, IOSInformation os, bool isRelative)
    {
        // Path has already been sanitized.
        if (IsSanitized(path, os, isRelative)) return path.ToString();

        // Paths without backslashes only need to be checked for trailing directory separators.
        // ReSharper disable once RedundantNameQualifier
        if (Reloaded.Memory.Extensions.SpanExtensions.Count(path, '\\') == 0)
        {
            var result = RemoveTrailingDirectorySeparator(path);
            AssertRootness(result, os, isRelative);

            return result.ToString();
        }

        // Paths with backslashes instead of forward slashes need to be fixed.
        var buffer = path.Length > 512
            ? GC.AllocateUninitializedArray<char>(path.Length)
            : stackalloc char[path.Length];

        var bufferIndex = 0;
        var previous = '\0';

        for (var pathIndex = 0; pathIndex < path.Length; pathIndex++)
        {
            var current = path.DangerousGetReferenceAt(pathIndex);
            if (previous == '\\' && current == '\\') continue;
            buffer[bufferIndex++] = current == '\\' ? '/' : current;
            previous = current;
        }

        var slice = buffer.SliceFast(0, bufferIndex);

        // Don't remove the trailing directory separator for root directories like "C:/".
        var output = IsRootDirectory(slice, os) ? slice : RemoveTrailingDirectorySeparator(slice);

        AssertRootness(output, os, isRelative);
        return output.ToString();
    }

    /// <summary>
    /// Verifies that relative paths aren't rooted and that absolute paths are rooted.
    /// </summary>
    /// <exception cref="PathException">Thrown when a relative path is rooted or an absolute path isn't rooted</exception>
    public static void AssertRootness(ReadOnlySpan<char> path, IOSInformation os, bool isRelative)
    {
        var isRooted = IsRooted(path, os);
        if (isRelative && isRooted) throw new PathException($"Relative path can't be rooted: `{path.ToString()}`");
        if (!isRooted && !isRelative) throw new PathException($"Absolute path must be rooted: `{path.ToString()}`");
    }

    /// <summary>
    /// Gets the root type of a path.
    /// </summary>
    public static PathRootType GetRootType(ReadOnlySpan<char> path) => GetPathRoot(path).RootType;

    /// <summary>
    /// Checks whether the given path is rooted.
    /// </summary>
    public static bool IsRooted(ReadOnlySpan<char> path, out PathRootType rootType)
    {
        rootType = GetRootType(path);
        return rootType != PathRootType.None;
    }

    /// <summary>
    /// Returns the root part of a path.
    /// </summary>
    /// <exception cref="PathException">Thrown for invalid paths</exception>
    public static PathRoot GetPathRoot(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty) return new PathRoot(ReadOnlySpan<char>.Empty, PathRootType.None);

        // DOS paths and relative paths don't start with a `/`
        if (path.DangerousGetReferenceAt(0) is not DirectorySeparatorChar)
        {
            // check for DOS path `C:/`
            if (path.Length < PathRoot.DOSRootLength) return new PathRoot(ReadOnlySpan<char>.Empty, PathRootType.None);

            var hasVolumeSeparator = path.DangerousGetReferenceAt(1) is WindowsVolumeSeparatorChar;
            var hasDirectorySeparator = path.DangerousGetReferenceAt(2) is DirectorySeparatorChar;
            if (!hasVolumeSeparator || !hasDirectorySeparator) return new PathRoot(ReadOnlySpan<char>.Empty, PathRootType.None);

            var windowsDriveChar = path.DangerousGetReferenceAt(0);
            if (!IsValidWindowsDriveChar(windowsDriveChar)) throw new PathException($"Path contains invalid windows drive character: `{path.ToString()}` (`{windowsDriveChar}`)");
            return new PathRoot(path.SliceFast(start: 0, length: PathRoot.DOSRootLength), PathRootType.DOS);
        }

        if (path.Length == 1) return new PathRoot(path.SliceFast(start: 0, length: 1), PathRootType.Unix);

        // UNC and DOS device paths start with `//`
        if (path.DangerousGetReferenceAt(1) is not DirectorySeparatorChar) return new PathRoot(path.SliceFast(start: 0, length: 1), PathRootType.Unix);

        // path starts with `//` and then has a random character, that's not valid
        if (path.Length < PathRoot.MinUNCRootLength) throw new PathException($"Path is too small to be a valid rooted path: `{path.ToString()}`");

        // DOS device paths start with either `//./` or `//?/`
        var dosDevicePathSeparatorChar = path.DangerousGetReferenceAt(2);
        var hasDOSDevicePathSeparatorChar = dosDevicePathSeparatorChar is '.' or '?';
        var isDOSDevicePath = path.Length >= PathRoot.DOSDeviceDriveRootLength && hasDOSDevicePathSeparatorChar && path.DangerousGetReferenceAt(3) is DirectorySeparatorChar;

        if (!isDOSDevicePath)
        {
            // check if UNC `//Server/foo`
            var slice = path.SliceFast(start: 2);
            var separatorIndex = slice.IndexOf(DirectorySeparatorChar);
            if (separatorIndex == -1) throw new PathException($"Invalid UNC path, missing directory separator: `{path.ToString()}`");

            Debug.Assert(path.Length >= 2 + separatorIndex + 1);
            var rootPart = path.SliceFast(start: 0, length: 2 + separatorIndex + 1);
            return new PathRoot(rootPart, PathRootType.UNC);
        }

        // check for DOS device drive paths `//./C:/`
        if (path.DangerousGetReferenceAt(5) is WindowsVolumeSeparatorChar && path.DangerousGetReferenceAt(6) is DirectorySeparatorChar)
        {
            var windowsDriveChar = path.DangerousGetReferenceAt(4);
            if (!IsValidWindowsDriveChar(windowsDriveChar)) throw new PathException($"Path contains invalid windows drive character: `{path.ToString()}` (`{windowsDriveChar}`)");
            return new PathRoot(path.SliceFast(start: 0, length: PathRoot.DOSDeviceDriveRootLength), PathRootType.DOSDeviceDrive);
        }

        if (path.Length < PathRoot.DOSDeviceVolumeRootLength) throw new PathException($"Path is not a valid DOS Device Volume path: `{path.ToString()}`");

        var hasVolumePrefix = path.SliceFast(start: PathRoot.DOSDevicePrefixLength, length: PathRoot.DOSDeviceVolumePrefix.Length).SequenceEqual(PathRoot.DOSDeviceVolumePrefix);
        if (!hasVolumePrefix) throw new PathException($"Path is missing DOS Device Volume prefix: `{path.ToString()}`");

        if (path.DangerousGetReferenceAt(PathRoot.DOSDeviceVolumeRootLength - 2) is not '}') throw new PathException($"Invalid DOS Device Volume path, missing directory separator: `{path.ToString()}`");
        return new PathRoot(path.SliceFast(start: 0, length: PathRoot.DOSDeviceVolumeRootLength), PathRootType.DOSDeviceVolume);
    }

    /// <summary>
    /// Replaces all directory separator characters with the
    /// native directory separator character of the passed OS.
    /// <remarks>
    /// Assumes sanitized path, changes to `/` on Unix-based systems and `\` on Windows.
    /// </remarks>
    /// </summary>
    [SkipLocalsInit]
    public static string ToNativeSeparators(ReadOnlySpan<char> path, IOSInformation os)
    {
        DebugAssertIsSanitized(path, os);
        if (path.IsEmpty) return string.Empty;

        if (os.IsUnix()) return path.ToString();

        var buffer = path.Length > 512
            ? GC.AllocateUninitializedArray<char>(path.Length)
            : stackalloc char[path.Length];
        path.CopyTo(buffer);
        return buffer.Replace('/', '\\', buffer).ToString();
    }

    /// <summary>
    /// Checks for equality between two paths.
    /// </summary>
    /// <remarks>
    /// Equality of paths is handled case-insensitive, meaning "/foo" is equal to "/FOO".
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PathEquals(ReadOnlySpan<char> left, ReadOnlySpan<char> right, IOSInformation os)
    {
        DebugAssertIsSanitized(left, os);
        DebugAssertIsSanitized(right, os);

        if (left.IsEmpty && right.IsEmpty) return true;
        if (left.IsEmpty && !right.IsEmpty || right.IsEmpty && !left.IsEmpty) return false;
        return left.Equals(right, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Compares two paths.
    /// </summary>
    /// <remarks>
    /// Path comparisons are handled case-insensitive, meaning "/foo" is equal to "/FOO".
    /// </remarks>
    /// <returns>
    /// A signed integer that indicates the relative order of <paramref name="left" /> and <paramref name="right" />:
    /// <br />   - If less than 0, <paramref name="left" /> precedes <paramref name="right" />.
    /// <br />   - If 0, <paramref name="left" /> equals <paramref name="right" />.
    /// <br />   - If greater than 0, <paramref name="left" /> follows <paramref name="right" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(ReadOnlySpan<char> left, ReadOnlySpan<char> right, IOSInformation os)
    {
        DebugAssertIsSanitized(left, os);
        DebugAssertIsSanitized(right, os);
        return left.CompareTo(right, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true if the given character is a valid Windows drive letter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidWindowsDriveChar(char value)
    {
        // Licensed to the .NET Foundation under one or more agreements.
        // The .NET Foundation licenses this file to you under the MIT license.
        // https://github.com/dotnet/runtime/blob/main/LICENSE.TXT
        // source: https://github.com/dotnet/runtime/blob/d9f453924f7c3cca9f02d920a57e1477293f216e/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L69-L75
        return (uint)((value | 0x20) - 'a') <= 'z' - 'a';
    }

    /// <summary>
    /// Calculates the length of the root of the path.
    /// </summary>
    /// <remarks>
    /// Unix-based systems have a root length of <c>1</c>. Currently, only volume paths
    /// are supported on Windows, which have a root length of <c>3</c>.
    /// </remarks>
    /// <returns>Returns the length of the root of the path or <c>-1</c> if the path is not rooted.</returns>
    /// <seealso cref="IsRooted"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetRootLength(ReadOnlySpan<char> path, IOSInformation os)
    {
        const int invalidLength = -1;
        const int unixLength = 1;
        const int windowsLength = 3;

        if (os.IsUnix())
        {
            // Unix-based systems start with '/'.
            return path.Length >= unixLength && path.DangerousGetReferenceAt(0) == DirectorySeparatorChar
                ? unixLength
                : invalidLength;
        }

        // Currently, only volume paths are supported: "C:/" on Windows.
        // UNC paths (\\?\C:\) and Device paths (\\?\.) are not supported.
        if (path.Length < windowsLength ||
            path.DangerousGetReferenceAt(1) != WindowsVolumeSeparatorChar ||
            !IsValidWindowsDriveChar(path.DangerousGetReferenceAt(0))) return invalidLength;

        return path.DangerousGetReferenceAt(2) == DirectorySeparatorChar ? windowsLength : invalidLength;
    }

    /// <summary>
    /// Checks whether the path is rooted.
    /// </summary>
    /// <seealso cref="GetRootLength"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRooted(ReadOnlySpan<char> path, IOSInformation os)
    {
        var rootLength = GetRootLength(path, os);
        return rootLength != -1;
    }

    /// <summary>
    /// Gets the root part of the path.
    /// </summary>
    /// <remarks>
    /// On Unix-based systems, the root part for rooted paths is always '/'.
    /// On Windows, the root part for rooted paths is the volume part: "C:/" and always ends with '/'.
    /// </remarks>
    /// <returns>For rooted paths, this returns the root part, for non-rooted paths, this returns <see cref="ReadOnlySpan{T}.Empty"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetRootPart(ReadOnlySpan<char> path, IOSInformation os)
    {
        var rootLength = GetRootLength(path, os);
        return rootLength == -1 ? ReadOnlySpan<char>.Empty : path.SliceFast(0, rootLength);
    }

    /// <summary>
    /// Checks whether the given path is a root directory.
    /// </summary>
    /// <seealso cref="IsRooted"/>
    /// <seealso cref="GetRootPart"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRootDirectory(ReadOnlySpan<char> path, IOSInformation os)
    {
        var rootLength = GetRootLength(path, os);
        return rootLength == path.Length;
    }

    /// <summary>
    /// Calculates the exact length required for a buffer to contain the result of
    /// joining two path parts using <see cref="JoinParts(System.Span{char},System.ReadOnlySpan{char},System.ReadOnlySpan{char},NexusMods.Paths.IOSInformation?)"/>.
    /// </summary>
    /// <seealso cref="GetMaxJoinedPartLength"/>
    public static int GetExactJoinedPartLength(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        if (left.IsEmpty) return right.Length;
        if (right.IsEmpty) return left.Length;
        if (left.DangerousGetReferenceAt(left.Length - 1) == DirectorySeparatorChar)
            return left.Length + right.Length;
        return left.Length + DirectorySeparatorString.Length + right.Length;
    }

    /// <summary>
    /// Gets the maximum length required for a buffer to contain the result of
    /// joining two path parts using <see cref="JoinParts(System.Span{char},System.ReadOnlySpan{char},System.ReadOnlySpan{char},NexusMods.Paths.IOSInformation?)"/>.
    /// </summary>
    /// <remarks>
    /// This method differs from <see cref="GetExactJoinedPartLength"/> in that it's the
    /// maximum amount, rather than the exact amount required. Using the maximum amount
    /// means potentially allocating more memory than required.
    /// </remarks>
    /// <seealso cref="GetExactJoinedPartLength"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMaxJoinedPartLength(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        return left.Length + DirectorySeparatorString.Length + right.Length;
    }

    /// <summary>
    /// Joins two path parts together and writes the result to a buffer.
    /// </summary>
    /// <remarks>
    /// This method returns the amount of written characters to the buffer.
    /// It's the responsibility of the caller to allocate enough memory for the buffer.
    /// Use <see cref="GetExactJoinedPartLength"/> to get an accurate length or
    /// <see cref="GetMaxJoinedPartLength"/> to get the maximum length.
    /// </remarks>
    /// <returns>The amount of written characters.</returns>
    public static int JoinParts(Span<char> buffer, ReadOnlySpan<char> left, ReadOnlySpan<char> right, IOSInformation os)
    {
        DebugAssertIsSanitized(left, os);
        DebugAssertIsSanitized(right, os);
        Debug.Assert(buffer.Length >= GetExactJoinedPartLength(left, right), $"Buffer has a size of '{buffer.Length}' but requires at least '{GetExactJoinedPartLength(left, right)}'");

        if (left.IsEmpty)
        {
            if (right.IsEmpty) return 0;

            right.CopyTo(buffer);
            return right.Length;
        }

        if (right.IsEmpty)
        {
            left.CopyTo(buffer);
            return left.Length;
        }

        if (left.DangerousGetReferenceAt(left.Length - 1) == DirectorySeparatorChar)
        {
            left.CopyTo(buffer);
            right.CopyTo(buffer.SliceFast(left.Length));
            return left.Length + right.Length;
        }

        left.CopyTo(buffer);

        ref var c = ref buffer.DangerousGetReferenceAt(left.Length);
        c = DirectorySeparatorChar;

        right.CopyTo(buffer.SliceFast(left.Length + DirectorySeparatorString.Length));

        return left.Length + DirectorySeparatorString.Length + right.Length;
    }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    /// <summary>
    /// Joins two path parts together and returns the joined path as a string.
    /// </summary>
    /// <remarks>
    /// This method uses <see cref="ReadOnlySpan{T}"/>. If you have strings as parts,
    /// use <see cref="JoinParts(string, string,NexusMods.Paths.IOSInformation?)"/> instead.
    /// </remarks>
    /// <param name="left">The left part of the path as a <see cref="ReadOnlySpan{T}"/></param>
    /// <param name="right">The right part of the path as a <see cref="ReadOnlySpan{T}"/></param>
    /// <param name="os"></param>
    /// <returns>The joined path.</returns>
    /// <seealso cref="JoinParts(System.Span{char},System.ReadOnlySpan{char},System.ReadOnlySpan{char},NexusMods.Paths.IOSInformation?)"/>
    /// <seealso cref="JoinParts(string, string,NexusMods.Paths.IOSInformation?)"/>
    public static string JoinParts(ReadOnlySpan<char> left, ReadOnlySpan<char> right, IOSInformation os)
    {
        DebugAssertIsSanitized(left, os);
        DebugAssertIsSanitized(right, os);

        var spanLength = GetExactJoinedPartLength(left, right);
        unsafe
        {
            // Note: The two Span objects are on the Stack. We access them inside
            // string.Create, by dereferencing these items from the stack.

            // If a GC happens, the pointers inside these referenced items will be
            // moved, but our stack objects won't. Therefore, access like this without
            // an explicit pin is safe.
            // A similar trick also exists out there known as 'ref pinning'.

            // Don't believe me? Go crazy with `DOTNET_GCStress` 😉 - Sewer
            var @params = new JoinPartsParams
            {
                Left = &left,
                Right = &right,
                Os = os
            };

            return string.Create(spanLength, @params, (span, tuple) =>
            {
                var count = JoinParts(span, *tuple.Left, *tuple.Right, tuple.Os);
                Debug.Assert(count == spanLength, $"Calculated span length '{spanLength}' doesn't match actual span length '{count}'");
            });
        }
    }

    unsafe struct JoinPartsParams
    {
        internal ReadOnlySpan<char>* Left;
        internal ReadOnlySpan<char>* Right;
        internal IOSInformation Os;
    }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

    /// <summary>
    /// Joins two path parts together and returns the joined path as a string.
    /// </summary>
    /// <remarks>
    /// This method uses strings as inputs. If you have <see cref="ReadOnlySpan{T}"/>,
    /// use <see cref="JoinParts(System.ReadOnlySpan{char},System.ReadOnlySpan{char},NexusMods.Paths.IOSInformation?)"/>
    /// or <see cref="JoinParts(System.Span{char},System.ReadOnlySpan{char},System.ReadOnlySpan{char},NexusMods.Paths.IOSInformation?)"/>
    /// instead.
    /// </remarks>
    /// <param name="left">The left part of the path as a <see cref="string"/></param>
    /// <param name="right">The right part of the path as a <see cref="string"/></param>
    /// <param name="os"></param>
    /// <returns>The joined path.</returns>
    /// <seealso cref="JoinParts(System.Span{char},System.ReadOnlySpan{char},System.ReadOnlySpan{char},NexusMods.Paths.IOSInformation?)"/>
    /// <seealso cref="JoinParts(System.ReadOnlySpan{char},System.ReadOnlySpan{char},NexusMods.Paths.IOSInformation?)"/>
    public static string JoinParts(string left, string right, IOSInformation os)
    {
        DebugAssertIsSanitized(left, os);
        DebugAssertIsSanitized(right, os);

        var spanLength = GetExactJoinedPartLength(left, right);
        return string.Create(spanLength, (left, right, os), (span, tuple) =>
        {
            // ReSharper disable InconsistentNaming
            var (left_, right_, os_) = tuple;
            // ReSharper restore InconsistentNaming

            var count = JoinParts(span, left_, right_, os_);
            Debug.Assert(count == spanLength, $"Calculated span length '{spanLength}' doesn't match actual span length '{count}'");
        });
    }

    /// <summary>
    /// Returns the file name of the given path or <see cref="ReadOnlySpan{T}.Empty"/>
    /// if there is no file name.
    /// </summary>
    /// <remarks>
    /// The file name is the last part of the path, after the last
    /// directory separator character. As such, if the path ends
    /// with a directory separator, the result will be <see cref="ReadOnlySpan{T}.Empty"/>.
    /// </remarks>
    /// <returns></returns>
    public static ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path, IOSInformation os)
    {
        DebugAssertIsSanitized(path, os);

        if (path.IsEmpty) return ReadOnlySpan<char>.Empty;
        if (path.DangerousGetReferenceAt(path.Length - 1) == DirectorySeparatorChar)
            return ReadOnlySpan<char>.Empty;

        for (var i = path.Length; --i >= 0;)
        {
            if (path.DangerousGetReferenceAt(i) != DirectorySeparatorChar) continue;
            return path.SliceFast(i + 1);
        }

        return path;
    }

    /// <summary>
    /// Returns the extension of the given path, or <see cref="ReadOnlySpan{T}.Empty"/>
    /// if there is no extension. The returned extension will always start with <see cref="ExtensionSeparatorChar"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty) return ReadOnlySpan<char>.Empty;
        if (path.DangerousGetReferenceAt(path.Length - 1) == ExtensionSeparatorChar)
            return ReadOnlySpan<char>.Empty;

        for (var i = path.Length; --i >= 0;)
        {
            if (path.DangerousGetReferenceAt(i) != ExtensionSeparatorChar) continue;
            return path.SliceFast(i);
        }

        return ReadOnlySpan<char>.Empty;
    }

    /// <summary>
    /// Replaces the extension of the old path with the new extension.
    /// </summary>
    /// <param name="oldPath"></param>
    /// <param name="newExtension"></param>
    /// <returns></returns>
    public static string ReplaceExtension(string oldPath, string newExtension)
    {
        var oldPathSpan = oldPath.AsSpan();
        if (oldPathSpan.IsEmpty) return string.Empty;

        int i;
        for (i = oldPathSpan.Length; --i >= 0;)
        {
            if (oldPathSpan.DangerousGetReferenceAt(i) == ExtensionSeparatorChar) break;
        }

        var oldPathWithoutExtensionLength = i > 0 ? i : oldPathSpan.Length;
        var newPathLength = oldPathWithoutExtensionLength + newExtension.Length;

        return string.Create(newPathLength, (oldPathWithoutExtensionLength, oldPath, newExtension), (span, tuple) =>
        {
            // ReSharper disable InconsistentNaming
            var (length, oldPath_, newExtension_) = tuple;
            // ReSharper restore InconsistentNaming

            var slice = oldPath_.AsSpan().SliceFast(0, length);
            slice.CopyTo(span);
            newExtension_.CopyTo(span.SliceFast(length));
        });
    }

    /// <summary>
    /// Calculates the depth of a path.
    /// </summary>
    /// <remarks>
    /// The depth of a path is defined by the numbers of directories it has.
    /// This is equal to the amount of directory separator characters.
    /// </remarks>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetDirectoryDepth(ReadOnlySpan<char> path, IOSInformation os)
    {
        DebugAssertIsSanitized(path, os);
        // ReSharper disable once RedundantNameQualifier
        return Reloaded.Memory.Extensions.SpanExtensions.Count(path, DirectorySeparatorChar);
    }

    /// <summary>
    /// Returns the directory name of the given path, also known as the parent.
    /// </summary>
    public static ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path, IOSInformation os)
    {
        DebugAssertIsSanitized(path, os);
        if (path.IsEmpty) return ReadOnlySpan<char>.Empty;

        var rootLength = GetRootLength(path, os);
        if (path.Length == rootLength) return path;

        for (var i = path.Length; --i >= 0;)
        {
            if (path.DangerousGetReferenceAt(i) != DirectorySeparatorChar) continue;

            // NOTE(erri120): the root directory always ends with a directory separator character,
            // while other paths don't, eg:
            // C:/foo -> C:/
            // C:/foo/bar -> C:/foo
            // /foo -> /
            // /foo/bar -> /foo
            return path.SliceFast(0, i == rootLength - 1 ? rootLength : i);
        }

        return ReadOnlySpan<char>.Empty;
    }

    /// <summary>
    /// Determines whether <paramref name="child"/> is in folder <paramref name="parent"/>.
    /// </summary>
    /// <remarks>
    /// This method will return <c>false</c>, if either <paramref name="child"/> or <paramref name="parent"/>
    /// are empty.
    /// </remarks>
    public static bool InFolder(ReadOnlySpan<char> child, ReadOnlySpan<char> parent, IOSInformation os)
    {
        DebugAssertIsSanitized(child, os);
        DebugAssertIsSanitized(parent, os);

        if (parent.IsEmpty || child.IsEmpty && parent.IsEmpty) return true;
        if (child.IsEmpty) return false;
        if (!child.StartsWith(parent, StringComparison.OrdinalIgnoreCase)) return false;

        if (child.Length == parent.Length) return true;
        if (IsRootDirectory(parent, os)) return true;
        return child.DangerousGetReferenceAt(parent.Length) == DirectorySeparatorChar;
    }

    /// <summary>
    /// Returns the part from <paramref name="child"/> that is relative to <paramref name="parent"/>.
    /// </summary>
    /// <remarks>
    /// This method will return <see cref="ReadOnlySpan{T}.Empty"/> if <paramref name="child"/> is
    /// not relative to <paramref name="parent"/>. This comparison is done using <see cref="InFolder"/>.
    /// </remarks>
    public static ReadOnlySpan<char> RelativeTo(ReadOnlySpan<char> child, ReadOnlySpan<char> parent, IOSInformation os)
    {
        DebugAssertIsSanitized(child, os);
        DebugAssertIsSanitized(parent, os);

        if (child.IsEmpty && parent.IsEmpty) return ReadOnlySpan<char>.Empty;
        if (!InFolder(child, parent, os)) return ReadOnlySpan<char>.Empty;

        return IsRootDirectory(parent, os)
            ? child.SliceFast(parent.Length)
            : child.SliceFast(parent.Length + DirectorySeparatorString.Length);
    }

    /// <summary>
    /// Returns the first directory in the path.
    /// </summary>
    public static ReadOnlySpan<char> GetTopParent(ReadOnlySpan<char> path, IOSInformation os)
    {
        DebugAssertIsSanitized(path, os);

        var rootPart = GetRootPart(path, os);
        if (!rootPart.IsEmpty) return rootPart;

        var index = path.IndexOf(DirectorySeparatorChar);
        return index == -1 ? path : path.SliceFast(0, index);
    }

    /// <summary>
    /// Drops the first <paramref name="count"/> parents of the given path.
    /// </summary>
    public static ReadOnlySpan<char> DropParents(ReadOnlySpan<char> path, int count, IOSInformation os)
    {
        DebugAssertIsSanitized(path, os);

        if (path.IsEmpty) return ReadOnlySpan<char>.Empty;
        if (count == 0) return path;
        if (IsRootDirectory(path, os)) return ReadOnlySpan<char>.Empty;

        var res = path;
        for (var x = 0; x < count; x++)
        {
            var index = res.IndexOf(DirectorySeparatorChar);
            if (index == -1) return ReadOnlySpan<char>.Empty;

            res = res.SliceFast(index + 1);
        }

        return res;
    }

    /// <summary>
    /// Delegate used with <see cref="PathHelpers.WalkParts"/>.
    /// </summary>
    /// <seealso cref="WalkPartDelegate{TState}"/>
    public delegate bool WalkPartDelegate(ReadOnlySpan<char> part);

    /// <summary>
    /// Delegate used with <seealso cref="PathHelpers.WalkParts{TState}"/>
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <seealso cref="WalkPartDelegate"/>
    public delegate bool WalkPartDelegate<TState>(ReadOnlySpan<char> part, ref TState state);

    /// <summary>
    /// Walks the parts of a path, invoking <paramref name="partDelegate"/> with each part of the path.
    /// </summary>
    /// <seealso cref="WalkParts{TState}"/>
    public static void WalkParts(ReadOnlySpan<char> path, WalkPartDelegate partDelegate, IOSInformation os, bool reverse = false)
    {
        WalkParts(path, ref reverse, (ReadOnlySpan<char> part, ref bool _) => partDelegate(part), os, reverse);
    }

    /// <summary>
    /// Walks the parts of a path, invoking <paramref name="partDelegate"/> with each part of the path.
    /// </summary>
    /// <remarks>
    /// The path <c>/foo</c> has the parts <c>/</c> and <c>foo</c>.
    /// </remarks>
    /// <param name="path">The path to walk.</param>
    /// <param name="state">The state to pass to the <paramref name="partDelegate"/></param>
    /// <param name="partDelegate">The delegate to invoke with each part and state.</param>
    /// <param name="reverse">Whether to walk the path forward or backwards.</param>
    /// <param name="os"></param>
    /// <typeparam name="TState"></typeparam>
    /// <seealso cref="WalkParts"/>
    public static void WalkParts<TState>(
        ReadOnlySpan<char> path,
        ref TState state,
        WalkPartDelegate<TState> partDelegate,
        IOSInformation os,
        bool reverse = false)
    {
        DebugAssertIsSanitized(path, os);

        if (path.IsEmpty)
        {
            partDelegate(ReadOnlySpan<char>.Empty, ref state);
            return;
        }

        var rootLength = GetRootLength(path, os);
        if (path.Length == rootLength)
        {
            partDelegate(path, ref state);
            return;
        }

        if (reverse) WalkPartsBackwards(path, rootLength, partDelegate, ref state);
        else WalkPartsForward(path, rootLength, partDelegate, ref state);
    }

    private static void WalkPartsForward<TState>(ReadOnlySpan<char> path, int rootLength, WalkPartDelegate<TState> @delegate, ref TState state)
    {
        var previous = 0;
        for (var i = 0; i < path.Length; i++)
        {
            if (path.DangerousGetReferenceAt(i) != DirectorySeparatorChar) continue;
            if (i + 1 == rootLength)
            {
                var slice = path.SliceFast(0, rootLength);
                if (!@delegate(slice, ref state)) return;
                previous = i + 1;
            }
            else
            {
                var slice = path.SliceFast(previous, i - previous);
                if (!@delegate(slice, ref state)) return;
                previous = i + 1;
            }
        }

        var rest = path.SliceFast(previous);
        @delegate(rest, ref state);
    }

    private static void WalkPartsBackwards<TState>(ReadOnlySpan<char> path, int rootLength, WalkPartDelegate<TState> @delegate, ref TState state)
    {
        var previous = path.Length;
        for (var i = path.Length; --i >= 0;)
        {
            if (path.DangerousGetReferenceAt(i) != DirectorySeparatorChar) continue;

            if (i + 1 == rootLength)
            {
                var slice = path.SliceFast(rootLength, previous - rootLength);
                if (!@delegate(slice, ref state)) return;
                previous = i + 1;
            }
            else
            {
                var slice = path.SliceFast(i + 1, previous - i - 1);
                if (!@delegate(slice, ref state)) return;
                previous = i;
            }
        }

        var rest = path.SliceFast(0, previous);
        @delegate(rest, ref state);
    }

    /// <summary>
    /// Returns a read-only list containing all parts of the given path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="reverse"></param>
    /// <param name="os"></param>
    /// <returns></returns>
    /// <seealso cref="WalkParts"/>
    /// <seealso cref="WalkParts{TState}"/>
    public static IReadOnlyList<string> GetParts(ReadOnlySpan<char> path, IOSInformation os, bool reverse = false)
    {
        DebugAssertIsSanitized(path, os);

        var list = new List<string>();

        WalkParts(path, ref list, (ReadOnlySpan<char> part, ref List<string> output) =>
        {
            output.Add(part.ToString());
            return true;
        }, os, reverse);

        return list;
    }
}
