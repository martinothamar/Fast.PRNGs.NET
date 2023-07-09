[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/martinothamar/Fast.PRNGs.NET/build.yml?branch=main)](https://github.com/martinothamar/Fast.PRNGs.NET/actions)
[![GitHub](https://img.shields.io/github/license/martinothamar/Fast.PRNGs.NET?style=flat-square)](https://github.com/martinothamar/Fast.PRNGs.NET/blob/main/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/Fast.PRNGs?style=flat-square)](https://www.nuget.org/packages/Fast.PRNGs/)<br/>
[![NuGet](https://img.shields.io/nuget/v/Fast.PRNGs?label=Fast.PRNGs)](https://www.nuget.org/packages/Fast.PRNGs)<br/>

## Fast.PRNGs.NET

Experiment - fast PRNG implementations in .NET.
Your PRNG is unlikely to be a bottleneck in anything you do, but there are exceptions like for instance Monte Carlo simulations, where generating random samples can take some time.

To be clear - there is no original work here, only .NET implementations of existing algorithms.
Mainly for learning/curiosity purposes.

Sources:
* https://prng.di.unimi.it/
* https://espadrine.github.io/blog/posts/shishua-the-fastest-prng-in-the-world.html
* 

### Benchmarks

The benchmarks measure generation of `double`s.
Iterations = `double`s per op.

There is likely overhead in capturing hardware counters, so these should be more "correct"

![Scaling iterations](/img/perf-scaling-2.png "Scaling iterations")

### Design

* Zero allocations (vectorized PRNGs may allocate during constructions since they may be buffered, see Shishua)
* Implemented as `struct`s - cache locality
* Inline as much as possible - no virtual calls/indirection (and if something isn't inlined, the above also helps)
* No abstraction - interfaces etc, makes it easier to not invalidate the above
* Vectorization where possible - beneficial if PRNG is on your hotpath

### Running tests

```pwsh
dotnet test -c Release --logger:"console;verbosity=detailed"
```

Plotly diagrams are generated during tests where distritution is compared to `System.Random` as a baseline.
The goal is for the implemented PRNGs to match the (uniform) distribution of `System.Random`.

