using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Fast.PRNGs;

internal static class Common
{
    // From http://prng.di.unimi.it/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double ExtractDouble(ulong value) =>
        (value >> 11) * (1.0 / (1ul << 53));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float ExtractSingle(ulong value) =>
        (value >> 40) * (1.0f / (1u << 24));

    private static readonly Vector256<double> DoubleMultiplier256 = Vector256.Create(1.0 / (1ul << 53));
    private static readonly Vector256<float> SingleMultiplier256 = Vector256.Create(1.0f / (1u << 24));
    private static readonly Vector512<double> DoubleMultiplier512 = Vector512.Create(1.0 / (1ul << 53));
    private static readonly Vector512<float> SingleMultiplier512 = Vector512.Create(1.0f / (1u << 24));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ExtractDoubles256(in Vector256<ulong> values, ref Vector256<double> result) =>
        result = Avx2.Multiply(Vector256.ConvertToDouble(Avx2.ShiftRightLogical(values, 11)), DoubleMultiplier256);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ExtractSingles256(in Vector256<ulong> values, ref Vector256<float> result) =>
        result = Avx2.Multiply(Vector256.ConvertToSingle(Avx2.ShiftRightLogical(values, 40).AsInt32()), SingleMultiplier256);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ExtractDoubles512(in Vector512<ulong> values, ref Vector512<double> result) =>
        result = Avx512F.Multiply(Vector512.ConvertToDouble(Avx512F.ShiftRightLogical(values, 11)), DoubleMultiplier512);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ExtractSingles512(in Vector512<ulong> values, ref Vector512<float> result) =>
        result = Avx512F.Multiply(Vector512.ConvertToSingle(Avx512F.ShiftRightLogical(values, 40).AsInt32()), SingleMultiplier512);


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
