using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Fast.PRNGs.Common;

namespace Fast.PRNGs;

/// <summary>
/// Implementation of the Shishua PRNG
/// Ported from: https://github.com/espadrine/shishua/blob/ecc72d5fef9ff49507924ad6e35ec67bf925bc88/shishua-avx2.h
/// This uses vectorization from the AVX2 instructions
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Shishua : IDisposable
{
    private static Span<ulong> Phi => new ulong[]
    {
        0x9E3779B97F4A7C15, 0xF39CC0605CEDC834, 0x1082276BF3A27251, 0xF86C6A11D0C18E95,
        0x2767F0B153D27B7F, 0x0347045B5BF1827F, 0x01886F0928403002, 0xC1D64BA40F335E36,
        0xF06AD7AE9717877E, 0x85839D6EFFBD7DC6, 0x64D325D1C5371682, 0xCADD0CCCFDFFBBE1,
        0x626E33B8D04B4331, 0xBBF73C790D94F79D, 0x471C4AB3ED3D82A5, 0xFEC507705E4AE6E5,
    };

    private const int BufferSize = 1 << 18;

    private readonly nuint _state;

    public static bool IsSupported => Avx2.IsSupported;

    unsafe private ref BufferedState State
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Unsafe.AsRef<BufferedState>(_state.ToPointer());
        }
    }

    private Shishua(ref Seed seed)
    {
        _state = AllocateState(ref seed);
    }

    public static Shishua Create()
    {
        ThrowIfNotSupported();

        Seed seed = default;
        var seedGenerator = Random.Shared;
        seedGenerator.NextBytes(MemoryMarshal.AsBytes(seed.Span));
        return new Shishua(ref seed);
    }

    public static Shishua Create(Random seedGenerator)
    {
        ThrowIfNotSupported();

        Seed seed = default;
        seedGenerator.NextBytes(MemoryMarshal.AsBytes(seed.Span));
        return new Shishua(ref seed);
    }

    public static Shishua Create(ReadOnlySpan<byte> seedBytes)
    {
        ThrowIfNotSupported();

        if (seedBytes.Length != 32)
            throw new ArgumentException("Seed bytes should be of length 32, got: " + seedBytes.Length);

        Seed seed = default;
        seedBytes.CopyTo(MemoryMarshal.AsBytes(seed.Span));
        return new Shishua(ref seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotSupported()
    {
        if (!Avx2.IsSupported)
            throw new InvalidOperationException("Need access to AVX2 instruction to run the Shishua PRNG");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextInternal()
    {
        const int size = sizeof(ulong);

        ref var bufferedState = ref this.State;
        if (bufferedState.BufferIndex >= BufferSize || BufferSize - bufferedState.BufferIndex < size)
        {
            FillBuffer(ref bufferedState);
        }

        var value = Unsafe.As<byte, ulong>(ref bufferedState.Buffer[bufferedState.BufferIndex]);
        bufferedState.BufferIndex += size;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Vector256<ulong> NextInternalVec256()
    {
        const int size = sizeof(ulong) * 4;

        ref var bufferedState = ref this.State;
        if (bufferedState.BufferIndex >= BufferSize || BufferSize - bufferedState.BufferIndex < size)
        {
            FillBuffer(ref bufferedState);
        }

        ref var value = ref Unsafe.As<byte, Vector256<ulong>>(ref bufferedState.Buffer[bufferedState.BufferIndex]);
        bufferedState.BufferIndex += size;
        return ref value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Vector512<ulong> NextInternalVec512()
    {
        const int size = sizeof(ulong) * 8;

        ref var bufferedState = ref this.State;
        if (bufferedState.BufferIndex >= BufferSize || BufferSize - bufferedState.BufferIndex < size)
        {
            FillBuffer(ref bufferedState);
        }

        ref var value = ref Unsafe.As<byte, Vector512<ulong>>(ref bufferedState.Buffer[bufferedState.BufferIndex]);
        bufferedState.BufferIndex += size;
        return ref value;
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
    public void NextDoubles256(ref Vector256<double> result)
    {
        result = Avx2.Multiply(Vector256.ConvertToDouble(Avx2.And(NextInternalVec256(), DoubleMaskVec256)), Norm53Vec256);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NextDoubles512(ref Vector512<double> result)
    {
        result = Avx512F.Multiply(Vector512.ConvertToDouble(Avx512F.And(NextInternalVec512(), DoubleMaskVec512)), Norm53Vec512);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat()
    {
        return (NextInternal() & FloatMask) * Norm24;
    }

    public void Dispose()
    {
        FreeState();
    }

    unsafe private nuint AllocateState(ref Seed seed)
    {
        void* ptr = NativeMemory.AlignedAlloc((nuint)Marshal.SizeOf<BufferedState>(), 128);
        ref BufferedState bufferedState = ref Unsafe.AsRef<BufferedState>(ptr);

        InitState(ref bufferedState.State, ref seed);
        FillBuffer(ref bufferedState);
        return (nuint)ptr;
    }

    private void InitState(ref RawState state, ref Seed seed)
    {
        state = default;

        const int steps = 1;
        const int rounds = 13;

        Span<byte> buf = stackalloc byte[128 * steps];

        state.State[0] = Vector256.Create((ulong)(Phi[0] ^ seed[0]), (ulong)(Phi[1]), (ulong)(Phi[2] ^ seed[1]), (ulong)(Phi[3]));
        state.State[1] = Vector256.Create((ulong)(Phi[4] ^ seed[2]), (ulong)(Phi[5]), (ulong)(Phi[6] ^ seed[3]), (ulong)(Phi[7]));
        state.State[2] = Vector256.Create((ulong)(Phi[8] ^ seed[2]), (ulong)(Phi[9]), (ulong)(Phi[10] ^ seed[3]), (ulong)(Phi[11]));
        state.State[3] = Vector256.Create((ulong)(Phi[12] ^ seed[0]), (ulong)(Phi[13]), (ulong)(Phi[14] ^ seed[1]), (ulong)(Phi[15]));

        for (int i = 0; i < rounds; i++)
        {
            PrngGen(ref state, buf);
            state.State[0] = state.Output[3]; state.State[1] = state.Output[2];
            state.State[2] = state.Output[1]; state.State[3] = state.Output[0];
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void FillBuffer(ref BufferedState bufferedState)
    {
        PrngGen(ref bufferedState.State, bufferedState.Buffer);
        bufferedState.BufferIndex = 0;
    }

    unsafe private void PrngGen(ref RawState state, Span<byte> buffer)
    {
        var size = buffer.Length;

        Vector256<ulong>
            o0 = state.Output[0], o1 = state.Output[1],
            o2 = state.Output[2], o3 = state.Output[3],
            s0 = state.State[0], s1 = state.State[1],
            s2 = state.State[2], s3 = state.State[3],
            t0, t1, t2, t3, u0, u1, u2, u3, counter = state.Counter;

        Vector256<uint> shu0 = Vector256.Create(5u, 6u, 7u, 0u, 1u, 2u, 3u, 4u),
                shu1 = Vector256.Create(3u, 4u, 5u, 6u, 7u, 0u, 1u, 2u);

        Vector256<ulong> increment = Vector256.Create(7UL, 5UL, 3UL, 1UL);

        Debug.Assert(size % 128 == 0, "buf's size must be a multiple of 128 bytes");

        for (int i = 0; i < size; i += 128)
        {
            if (!buffer.IsEmpty)
            {
                Avx.Store((ulong*)Unsafe.AsPointer(ref buffer[i + 0]), o0);
                Avx.Store((ulong*)Unsafe.AsPointer(ref buffer[i + 32]), o0);
                Avx.Store((ulong*)Unsafe.AsPointer(ref buffer[i + 64]), o0);
                Avx.Store((ulong*)Unsafe.AsPointer(ref buffer[i + 96]), o0);
            }

            s1 = Avx2.Add(s1, counter);
            s3 = Avx2.Add(s3, counter);
            counter = Avx2.Add(counter, increment);

            u0 = Avx2.ShiftRightLogical(s0, 1); u1 = Avx2.ShiftRightLogical(s1, 3);
            u2 = Avx2.ShiftRightLogical(s2, 1); u3 = Avx2.ShiftRightLogical(s3, 3);
            t0 = Avx2.PermuteVar8x32(s0.AsUInt32(), shu0).AsUInt64(); t1 = Avx2.PermuteVar8x32(s1.AsUInt32(), shu1).AsUInt64();
            t2 = Avx2.PermuteVar8x32(s2.AsUInt32(), shu0).AsUInt64(); t3 = Avx2.PermuteVar8x32(s3.AsUInt32(), shu1).AsUInt64();

            s0 = Avx2.Add(t0, u0); s1 = Avx2.Add(t1, u1);
            s2 = Avx2.Add(t2, u2); s3 = Avx2.Add(t3, u3);

            o0 = Avx2.Xor(u0, t1);
            o1 = Avx2.Xor(u2, t3);
            o2 = Avx2.Xor(s0, s3);
            o3 = Avx2.Xor(s2, s1);
        }

        state.Output[0] = o0; state.Output[1] = o1; state.Output[2] = o2; state.Output[3] = o3;
        state.State[0] = s0; state.State[1] = s1; state.State[2] = s2; state.State[3] = s3;
        state.Counter = counter;
    }

    unsafe private void FreeState()
    {
        NativeMemory.AlignedFree(_state.ToPointer());
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe private struct BufferedState
    {
        public RawState State;
        private fixed byte _buffer[BufferSize];
        public int BufferIndex;

        public Span<byte> Buffer => MemoryMarshal.CreateSpan(ref _buffer[0], BufferSize);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawState
    {
        private Vector256<ulong> _state00;
        private Vector256<ulong> _state01;
        private Vector256<ulong> _state02;
        private Vector256<ulong> _state03;
        public Span<Vector256<ulong>> State => MemoryMarshal.CreateSpan(ref _state00, 4);


        private Vector256<ulong> _output00;
        private Vector256<ulong> _output01;
        private Vector256<ulong> _output02;
        private Vector256<ulong> _output03;
        public Span<Vector256<ulong>> Output => MemoryMarshal.CreateSpan(ref _output00, 4);

        public Vector256<ulong> Counter;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe private struct Seed
    {
        private fixed ulong _value[4];

        public readonly ref ulong this[int i] => ref Unsafe.AsRef(in _value[i]);

        public Span<ulong> Span => MemoryMarshal.CreateSpan(ref _value[0], 4);
    }
}
