using System.Runtime.CompilerServices;

namespace Fast.PRNGs;

internal static class Common
{
    internal const ulong DoubleMask = (1L << 53) - 1;
    internal const double Norm53 = 1.0d / (1L << 53);
    internal const ulong FloatMask = (1L << 24) - 1;
    internal const float Norm24 = 1.0f / (1L << 24);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Rotl(ulong x, int k)
    {
        return (x << k) | (x >> (64 - k));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint NextUInt(this Random random)
    {
        return (uint)random.Next();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong NextULong(this Random random)
    {
        return ((((ulong)random.NextUInt()) << 32) + random.NextUInt());
    }
}
