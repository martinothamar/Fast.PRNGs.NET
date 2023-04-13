using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Fast.PRNGs.Common;

namespace Fast.PRNGs;

[StructLayout(LayoutKind.Sequential)]
public struct Xoshiro256Plus
{
    private ulong _state0;
    private ulong _state1;
    private ulong _state2;
    private ulong _state3;

    private Xoshiro256Plus(ulong state0, ulong state1, ulong state2, ulong state3)
    {
        _state0 = state0;
        _state1 = state1;
        _state2 = state2;
        _state3 = state3;
    }

    public static Xoshiro256Plus Create()
    {
        var seedGenerator = Splitmix64.Create();
        return new Xoshiro256Plus(
            seedGenerator.Next(),
            seedGenerator.Next(),
            seedGenerator.Next(),
            seedGenerator.Next()
        );
    }

    public static Xoshiro256Plus Create(Random seedGenerator)
    {
        return new Xoshiro256Plus(
            seedGenerator.NextUInt(),
            seedGenerator.NextUInt(),
            seedGenerator.NextUInt(),
            seedGenerator.NextUInt()
        );
    }

    public static Xoshiro256Plus Create(ReadOnlySpan<byte> seedBytes)
    {
        if (seedBytes.Length != 32)
            throw new ArgumentException("Seed bytes should be of length 32, got: " + seedBytes.Length);

        return new Xoshiro256Plus(
            Unsafe.Add(ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(seedBytes)), 0),
            Unsafe.Add(ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(seedBytes)), 1),
            Unsafe.Add(ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(seedBytes)), 2),
            Unsafe.Add(ref Unsafe.As<byte, ulong>(ref MemoryMarshal.GetReference(seedBytes)), 3)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextInternal()
    {
        var result = _state0 + _state3;

        var t = _state1 << 17;

        _state2 ^= _state0;
        _state3 ^= _state1;
        _state1 ^= _state2;
        _state0 ^= _state3;

        _state2 ^= t;

        _state3 = Rotl(_state3, 45);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Next()
    {
        return (int)(NextInternal() >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble()
    {
        return (NextInternal() & DoubleMask) * Norm53;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat()
    {
        return (NextInternal() & FloatMask) * Norm24;
    }
}
