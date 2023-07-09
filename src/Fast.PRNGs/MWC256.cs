using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Fast.PRNGs.Common;

namespace Fast.PRNGs;

/// <summary>
/// Implementation of the MWC 256 PRNG
/// Ported from: https://prng.di.unimi.it/MWC256.c
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MWC256
{
    private const ulong MWC_A3 = 0xfff62cf2ccc0cdaf;

    private ulong _x, _y, _z, _c;

    private MWC256(ulong x, ulong y, ulong z, ulong c)
    {
        _x = x;
        _y = y;
        _z = z;
        _c = c;
    }

    public static MWC256 Create()
    {
        var seedGenerator = Random.Shared;
        return new MWC256(
            seedGenerator.NextULong(),
            seedGenerator.NextULong(),
            seedGenerator.NextULong(),
            seedGenerator.NextULong() % (MWC_A3 - 1)
        );
    }

    public static MWC256 Create(Random seedGenerator)
    {
        return new MWC256(
            seedGenerator.NextULong(),
            seedGenerator.NextULong(),
            seedGenerator.NextULong(),
            seedGenerator.NextULong() % (MWC_A3 - 1)
        );
    }

    public static MWC256 Create(ReadOnlySpan<byte> seedBytes)
    {
        if (seedBytes.Length != 32)
            throw new ArgumentException("Seed bytes should be of length 32, got: " + seedBytes.Length);

        return new MWC256(
            Unsafe.Add(ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(seedBytes)), 0),
            Unsafe.Add(ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(seedBytes)), 1),
            Unsafe.Add(ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(seedBytes)), 2),
            Unsafe.Add(ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(seedBytes)), 3) % (MWC_A3 - 1)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextInternal()
    {
        var result = _z;
        UInt128 t = MWC_A3 * (UInt128)_x + _c;
        _x = _y;
        _y = _z;
        _z = (ulong)t;
        _c = (ulong)(t >> 64);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Next()
    {
        return (int)(NextInternal() >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble() => ExtractDouble(NextInternal());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat() => ExtractSingle(NextInternal());
}
