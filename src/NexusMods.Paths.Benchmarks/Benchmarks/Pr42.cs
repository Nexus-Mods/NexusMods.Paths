using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using NexusMods.Paths.Benchmarks.Interfaces;
using NexusMods.Paths.Utilities;

namespace NexusMods.Paths.Benchmarks.Benchmarks;

[BenchmarkInfo("PR42", "Comparison for PR #42")]
[MemoryDiagnoser]
public class Pr42 : IBenchmark
{
    [Params(@"short", @"a/very/long/path/indeed/to/see/how/it/performs")]
    public string Path1 { get; set; } = null!;

    [Params("short", "a/very/long/path/indeed/to/see/how/it/performs")]
    public string Path2 { get; set; } = null!;

    private readonly IOSInformation _osInformation;

    public Pr42()
    {
        _osInformation = OSInformation.Shared;
    }

    [Benchmark]
    public string Current()
    {
        return PathHelpers.JoinParts(Path1.AsSpan(), Path2.AsSpan());
    }

    [Benchmark]
    public string OldMethod()
    {
        return JoinParts(Path1.AsSpan(), Path2.AsSpan(), _osInformation);
    }

    [SkipLocalsInit] // original method didn't have it, but it's still good for comparison.
    private static string JoinParts(ReadOnlySpan<char> left, ReadOnlySpan<char> right, IOSInformation os)
    {
        var spanLength = PathHelpers.GetExactJoinedPartLength(left, right);
        var buffer = spanLength > 512
            ? GC.AllocateUninitializedArray<char>(spanLength)
            : stackalloc char[spanLength];

        var count = PathHelpers.JoinParts(buffer, left, right);
        if (count == 0) return string.Empty;
        return buffer.ToString();
    }
}
