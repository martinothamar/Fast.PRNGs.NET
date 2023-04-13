using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Fast.PRNGs.Common;

namespace Fast.PRNGs;

/// <summary>
/// Implementation of http://prng.di.unimi.it/xoroshiro128plus.c
/// One of the fastest prng's for 32bit/64bit floating points.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Xoroshiro128Plus
{
    private const int A = 24;
    private const int B = 16;
    private const int C = 37;

    private ulong _state0;
    private ulong _state1;

    private Xoroshiro128Plus(Random? seedGenerator = null)
    {
        seedGenerator ??= Random.Shared;
        const int min = int.MinValue;
        const int max = int.MaxValue;
        _state0 = (ulong)seedGenerator.Next(min, max) << 32 | (uint)seedGenerator.Next(min, max);
        _state1 = (ulong)seedGenerator.Next(min, max) << 32 | (uint)seedGenerator.Next(min, max);
    }

    public static Xoroshiro128Plus Create(Random? seedGenerator = null) =>
        new Xoroshiro128Plus(seedGenerator);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextInternal()
    {
        var s0 = _state0;
        var s1 = _state1 ^ s0;

        _state0 = Rotl(s0, A) ^ s1 ^ s1 << B;
        _state1 = Rotl(s1, C);

        return _state0 + _state1;
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
