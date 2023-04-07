using System.Runtime.CompilerServices;

namespace Fast.PRNGs;

public struct Splitmix64
{
    private ulong _x;

    private Splitmix64(ulong x)
    {
        _x = x;
    }

    public static Splitmix64 Create()
    {
        var seedGenerator = Random.Shared;
        return new Splitmix64(seedGenerator.NextULong());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        ulong z = (_x += 0x9e3779b97f4a7c15);
        z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
        z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
        return z ^ (z >> 31);
    }
}
