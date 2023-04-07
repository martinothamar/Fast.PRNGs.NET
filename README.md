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

The below benchmark is generating 100_000 and 1_000_000 `double`s per run.

``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.201
  [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2


```
|              Method | Iterations |       Mean |   Error |  StdDev |        Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
|-------------------- |----------- |-----------:|--------:|--------:|-------------:|--------:|-----:|----------:|------------:|
|     SystemRandomGen |     100000 |   284.0 μs | 0.62 μs | 0.55 μs |     baseline |         |    4 |         - |          NA |
| Xoroshiro128PlusGen |     100000 |   203.2 μs | 0.38 μs | 0.33 μs | 1.40x faster |   0.00x |    3 |         - |          NA |
|   Xoshiro256PlusGen |     100000 |   159.6 μs | 0.30 μs | 0.28 μs | 1.78x faster |   0.00x |    2 |         - |          NA |
|          ShishuaGen |     100000 |   104.2 μs | 0.63 μs | 0.59 μs | 2.73x faster |   0.02x |    1 |         - |          NA |
|                     |            |            |         |         |              |         |      |           |             |
|     SystemRandomGen |    1000000 | 2,847.6 μs | 3.27 μs | 2.90 μs |     baseline |         |    4 |       4 B |             |
| Xoroshiro128PlusGen |    1000000 | 2,039.1 μs | 6.19 μs | 5.79 μs | 1.40x faster |   0.00x |    3 |       4 B |  1.00x more |
|   Xoshiro256PlusGen |    1000000 | 1,601.5 μs | 2.78 μs | 2.60 μs | 1.78x faster |   0.00x |    2 |       2 B |  2.00x less |
|          ShishuaGen |    1000000 | 1,039.5 μs | 2.16 μs | 2.02 μs | 2.74x faster |   0.01x |    1 |       2 B |  2.00x less |
