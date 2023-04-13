## Fast.PRNGs.NET

Experiment - fast PRNG implementations in .NET.
Your PRNG is unlikely to be a bottleneck in anything you do, but there are exceptions like for instance Monte Carlo simulations, where generating random samples can take some time.

To be clear - there is no original work here, only .NET implementations of existing algorithms.
Mainly for learning/curiosity purposes.

Sources:
* https://prng.di.unimi.it/
* https://espadrine.github.io/blog/posts/shishua-the-fastest-prng-in-the-world.html

### Design

* Implemented as `struct`s - cache locality
* Inline as much as possible - no virtual calls/indirection (and if something isn't inlined, the above also helps)
* No abstraction - interfaces etc, makes it easier to not invalidate the above
* Vectorization where possible - beneficial if PRNG is on your hotpath

### Initial benchmarks

The benchmarks measure generation of `double`s.
Iterations = `double`s per op.

#### With hardware counters

``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1555/22H2/2022Update/SunValley2)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.300-preview.23122.5
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|              Method | Iterations |     Mean |   Error |  StdDev |        Ratio | RatioSD | Rank | BranchInstructions/Op | CacheMisses/Op | TotalCycles/Op | BranchMispredictions/Op | Allocated | Alloc Ratio |
|-------------------- |----------- |---------:|--------:|--------:|-------------:|--------:|-----:|----------------------:|---------------:|---------------:|------------------------:|----------:|------------:|
|     SystemRandomGen |     100000 | 423.5 μs | 7.62 μs | 7.13 μs |     baseline |         |    4 |               603,262 |            482 |      1,293,860 |                     716 |         - |          NA |
| Xoroshiro128PlusGen |     100000 | 305.8 μs | 2.99 μs | 2.80 μs | 1.38x faster |   0.03x |    3 |               107,806 |            261 |        999,642 |                     392 |         - |          NA |
|   Xoshiro256PlusGen |     100000 | 231.2 μs | 2.85 μs | 2.67 μs | 1.83x faster |   0.03x |    2 |               105,846 |            219 |        750,974 |                     308 |         - |          NA |
|          ShishuaGen |     100000 | 185.7 μs | 1.32 μs | 1.24 μs | 2.28x faster |   0.05x |    1 |               437,648 |            640 |        529,914 |                     346 |         - |          NA |


#### Scaling iterations

There is likely overhead in capturing hardware counters, so these should be more "correct"

``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1555/22H2/2022Update/SunValley2)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.300-preview.23122.5
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|              Method | Iterations |       Mean |    Error |   StdDev |        Ratio | RatioSD | Rank |
|-------------------- |----------- |-----------:|---------:|---------:|-------------:|--------:|-----:|
|     SystemRandomGen |     100000 |   282.1 μs |  2.04 μs |  1.70 μs |     baseline |         |    4 |
| Xoroshiro128PlusGen |     100000 |   213.2 μs |  0.45 μs |  0.37 μs | 1.32x faster |   0.01x |    3 |
|   Xoshiro256PlusGen |     100000 |   158.7 μs |  1.18 μs |  1.11 μs | 1.78x faster |   0.02x |    2 |
|          ShishuaGen |     100000 |   103.5 μs |  0.54 μs |  0.45 μs | 2.73x faster |   0.02x |    1 |
|                     |            |            |          |          |              |         |      |
|     SystemRandomGen |    1000000 | 2,825.6 μs | 18.07 μs | 16.90 μs |     baseline |         |    4 |
| Xoroshiro128PlusGen |    1000000 | 2,141.1 μs | 15.47 μs | 14.47 μs | 1.32x faster |   0.01x |    3 |
|   Xoshiro256PlusGen |    1000000 | 1,583.3 μs |  7.22 μs |  6.40 μs | 1.79x faster |   0.01x |    2 |
|          ShishuaGen |    1000000 | 1,030.8 μs |  8.83 μs |  8.26 μs | 2.74x faster |   0.03x |    1 |

