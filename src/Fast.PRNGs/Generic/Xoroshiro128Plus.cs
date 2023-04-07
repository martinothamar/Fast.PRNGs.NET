using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using InlineIL;

using static InlineIL.IL.Emit;

namespace Fast.PRNGs.Generic;

/// <summary>
/// Generic (over single/double precision floating point) implementation of http://prng.di.unimi.it/xoroshiro128plus.c
/// One of the fastest prng's for 32bit/64bit floating points.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Xoroshiro128Plus<TNumericType>
    where TNumericType : unmanaged, IBinaryFloatingPointIeee754<TNumericType>
{
    private static readonly ulong MASK;
    private static readonly TNumericType NORM;

    private const int A = 24;
    private const int B = 16;
    private const int C = 37;

    private ulong _state0;
    private ulong _state1;

    static Xoroshiro128Plus()
    {
        if (typeof(TNumericType) == typeof(float))
        {
            MASK = (1L << 24) - 1;
            NORM = TNumericType.CreateChecked(1.0) / TNumericType.CreateChecked(1L << 24);
        }
        else if (typeof(TNumericType) == typeof(double))
        {
            MASK = (1L << 53) - 1;
            NORM = TNumericType.CreateChecked(1.0) / TNumericType.CreateChecked(1L << 53);
        }
        else
            throw new InvalidProgramException();
    }

    private Xoroshiro128Plus(Random? seedGenerator = null)
    {
        if (seedGenerator is not null)
        {
            const int min = int.MinValue;
            const int max = int.MaxValue;
            _state0 = (ulong)seedGenerator.Next(min, max) << 32 | (uint)seedGenerator.Next(min, max);
            _state1 = (ulong)seedGenerator.Next(min, max) << 32 | (uint)seedGenerator.Next(min, max);
        }
        else
        {
            Unsafe.SkipInit(out ulong rng1);
            Unsafe.SkipInit(out uint rng2);
            Unsafe.SkipInit(out ulong rng3);
            Unsafe.SkipInit(out uint rng4);

            _state0 = rng1 << 32 | rng2;
            _state1 = rng3 << 32 | rng4;
        }
    }

    public static Xoroshiro128Plus<TNumericType> Create(Random? seedGenerator = null) =>
        new Xoroshiro128Plus<TNumericType>(seedGenerator);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static ulong Rotl(ulong x, int k)
    {
        return (x << k) | (x >> (64 - k));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private ulong NextInternal()
    {
        var s0 = _state0;
        var s1 = _state1 ^ s0;

        _state0 = Rotl(s0, A) ^ s1 ^ s1 << B;
        _state1 = Rotl(s1, C);

        return _state0 + _state1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Next()
    {
        return (int)(NextInternal() >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public TNumericType NextFloating()
    {
        // InlineIL to avoid CreateChecked on the generic floating type, which would add overhead..
        Ldarg_0();
        Call(new MethodRef(typeof(Xoroshiro128Plus<TNumericType>), nameof(NextInternal)));
        Ldsfld(new FieldRef(typeof(Xoroshiro128Plus<TNumericType>), nameof(MASK)));
        And();
        Conv_R_Un();
        Conv_R8();
        Ldsfld(new FieldRef(typeof(Xoroshiro128Plus<TNumericType>), nameof(NORM)));
        Mul();
        return IL.Return<TNumericType>();
    }
}
