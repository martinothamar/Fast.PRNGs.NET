namespace RawIntrinsics
{
	public static unsafe partial class SSE42
	{
		/// <summary>
		/// Compare packed signed 64-bit integers in "a" and "b" for greater-than, and store the results in "dst".
		/// </summary>
		/// <remarks><c>PCMPGTQ xmm, xmm</c></remarks>
		/// <param name="a"><c>__m128i {SI64}</c></param>
		/// <param name="b"><c>__m128i {SI64}</c></param>
		/// <returns><c>__m128i dst {UI64}</c></returns>
		public static __m128i _mm_cmpgt_epi64(__m128i a, __m128i b) => System.Runtime.Intrinsics.X86.Sse42.CompareGreaterThan(a.SI64, b.SI64);

		/// <summary>
		/// Starting with the initial value in "crc", accumulates a CRC32 value for unsigned 16-bit integer "v", and stores the result in "dst".
		/// </summary>
		/// <remarks><c>CRC32 r32, r16</c></remarks>
		/// <param name="crc"><c>int {UI32}</c></param>
		/// <param name="v"><c>short {UI16}</c></param>
		/// <returns><c>int dst {UI32}</c></returns>
		public static int _mm_crc32_u16(int crc, short v) => (int)System.Runtime.Intrinsics.X86.Sse42.Crc32((uint)crc, (ushort)v);

		/// <summary>
		/// Starting with the initial value in "crc", accumulates a CRC32 value for unsigned 32-bit integer "v", and stores the result in "dst".
		/// </summary>
		/// <remarks><c>CRC32 r32, r32</c></remarks>
		/// <param name="crc"><c>int {UI32}</c></param>
		/// <param name="v"><c>int {UI32}</c></param>
		/// <returns><c>int dst {UI32}</c></returns>
		public static int _mm_crc32_u32(int crc, int v) => (int)System.Runtime.Intrinsics.X86.Sse42.Crc32((uint)crc, (uint)v);

		/// <summary>
		/// Starting with the initial value in "crc", accumulates a CRC32 value for unsigned 64-bit integer "v", and stores the result in "dst".
		/// </summary>
		/// <remarks><c>CRC32 r64, r64</c></remarks>
		/// <param name="crc"><c>long {UI64}</c></param>
		/// <param name="v"><c>long {UI64}</c></param>
		/// <returns><c>long dst {UI64}</c></returns>
		public static long _mm_crc32_u64(long crc, long v) => (long)System.Runtime.Intrinsics.X86.Sse42.X64.Crc32((ulong)crc, (ulong)v);

		/// <summary>
		/// Starting with the initial value in "crc", accumulates a CRC32 value for unsigned 8-bit integer "v", and stores the result in "dst".
		/// </summary>
		/// <remarks><c>CRC32 r32, r8</c></remarks>
		/// <param name="crc"><c>int {UI32}</c></param>
		/// <param name="v"><c>byte {UI8}</c></param>
		/// <returns><c>int dst {UI32}</c></returns>
		public static int _mm_crc32_u8(int crc, byte v) => (int)System.Runtime.Intrinsics.X86.Sse42.Crc32((uint)crc, v);

	}
}
