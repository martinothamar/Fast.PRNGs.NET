using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Fast.PRNGs;

internal static class Common
{
    internal const ulong DoubleMask = (1L << 53) - 1;
    internal const double Norm53 = 1.0d / (1L << 53);
    internal const ulong FloatMask = (1L << 24) - 1;
    internal const float Norm24 = 1.0f / (1L << 24);


    internal static readonly Vector256<ulong> DoubleMaskVec256 = Vector256.Create(DoubleMask);
    internal static readonly Vector256<double> Norm53Vec256 = Vector256.Create(Norm53);

    internal static readonly Vector512<ulong> DoubleMaskVec512 = Vector512.Create(DoubleMask);
    internal static readonly Vector512<double> Norm53Vec512 = Vector512.Create(Norm53);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Rotl(ulong x, int k)
    {
        return (x << k) | (x >> (64 - k));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong NextULong(this Random random)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        return random.NextULong(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong NextULong(this Random random, Span<byte> bytes)
    {
        random.NextBytes(bytes);
        return Unsafe.ReadUnaligned<ulong>(ref bytes[0]);
    }
}
