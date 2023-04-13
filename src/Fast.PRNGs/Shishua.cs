using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using RawIntrinsics;

using static Fast.PRNGs.Common;
using static RawIntrinsics.AVX;
using static RawIntrinsics.AVX2;

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

    private const int BufferSize = 1 << 17;

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

        state.State[0] = _mm256_setr_epi64x((long)(Phi[0] ^ seed[0]), (long)(Phi[1]), (long)(Phi[2] ^ seed[1]), (long)(Phi[3]));
        state.State[1] = _mm256_setr_epi64x((long)(Phi[4] ^ seed[2]), (long)(Phi[5]), (long)(Phi[6] ^ seed[3]), (long)(Phi[7]));
        state.State[2] = _mm256_setr_epi64x((long)(Phi[8] ^ seed[2]), (long)(Phi[9]), (long)(Phi[10] ^ seed[3]), (long)(Phi[11]));
        state.State[3] = _mm256_setr_epi64x((long)(Phi[12] ^ seed[0]), (long)(Phi[13]), (long)(Phi[14] ^ seed[1]), (long)(Phi[15]));

        for (int i = 0; i < rounds; i++)
        {
            PrngGen(ref state, buf);
            state.State[0] = state.Output[3]; state.State[1] = state.Output[2];
            state.State[2] = state.Output[1]; state.State[3] = state.Output[0];
        }
    }

    private void FillBuffer(ref BufferedState bufferedState)
    {
        PrngGen(ref bufferedState.State, bufferedState.Buffer);
        bufferedState.BufferIndex = 0;
    }

    unsafe private void PrngGen(ref RawState state, Span<byte> buffer)
    {
        var size = buffer.Length;

        __m256i
            o0 = state.Output[0], o1 = state.Output[1],
            o2 = state.Output[2], o3 = state.Output[3],
            s0 = state.State[0], s1 = state.State[1],
            s2 = state.State[2], s3 = state.State[3],
            t0, t1, t2, t3, u0, u1, u2, u3, counter = state.Counter;

        __m256i shu0 = _mm256_setr_epi32(5, 6, 7, 0, 1, 2, 3, 4),
                shu1 = _mm256_setr_epi32(3, 4, 5, 6, 7, 0, 1 ,2);

        __m256i increment = _mm256_setr_epi64x(7, 5, 3, 1);

        Debug.Assert(size % 128 == 0, "buf's size must be a multiple of 128 bytes");

        for (int i = 0; i < size; i += 128)
        {
            if (!buffer.IsEmpty)
            {
                _mm256_storeu_si256((__m256i*)Unsafe.AsPointer(ref buffer[i + 0]), o0);
                _mm256_storeu_si256((__m256i*)Unsafe.AsPointer(ref buffer[i + 32]), o1);
                _mm256_storeu_si256((__m256i*)Unsafe.AsPointer(ref buffer[i + 64]), o2);
                _mm256_storeu_si256((__m256i*)Unsafe.AsPointer(ref buffer[i + 96]), o3);
            }

            s1 = _mm256_add_epi64(s1, counter);
            s3 = _mm256_add_epi64(s3, counter);
            counter = _mm256_add_epi64(counter, increment);

            u0 = _mm256_srli_epi64(s0, 1); u1 = _mm256_srli_epi64(s1, 3);
            u2 = _mm256_srli_epi64(s2, 1); u3 = _mm256_srli_epi64(s3, 3);
            t0 = _mm256_permutevar8x32_epi32(s0, shu0); t1 = _mm256_permutevar8x32_epi32(s1, shu1);
            t2 = _mm256_permutevar8x32_epi32(s2, shu0); t3 = _mm256_permutevar8x32_epi32(s3, shu1);

            s0 = _mm256_add_epi64(t0, u0); s1 = _mm256_add_epi64(t1, u1);
            s2 = _mm256_add_epi64(t2, u2); s3 = _mm256_add_epi64(t3, u3);

            o0 = _mm256_xor_si256(u0, t1);
            o1 = _mm256_xor_si256(u2, t3);
            o2 = _mm256_xor_si256(s0, s3);
            o3 = _mm256_xor_si256(s2, s1);
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
        private __m256i _state00;
        private __m256i _state01;
        private __m256i _state02;
        private __m256i _state03;
        public Span<__m256i> State => MemoryMarshal.CreateSpan(ref _state00, 4);


        private __m256i _output00;
        private __m256i _output01;
        private __m256i _output02;
        private __m256i _output03;
        public Span<__m256i> Output => MemoryMarshal.CreateSpan(ref _output00, 4);

        public __m256i Counter;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe private struct Seed
    {
        private fixed ulong _value[4];

        public readonly ref ulong this[int i] => ref Unsafe.AsRef(in _value[i]);

        public Span<ulong> Span => MemoryMarshal.CreateSpan(ref _value[0], 4);
    }
}