## Fast.PRNGs.NET

Experiment - fast PRNG implementations in .NET.
Your PRNG is unlikely to be a bottleneck in anything you do, but there are exceptions like for instance Monte Carlo simulations, where generating random samples can take some time.

To be clear - there is no original work here, only .NET implementations of existing algorithms.
Mainly for learning/curiosity purposes.

Sources:
* https://prng.di.unimi.it/
* https://espadrine.github.io/blog/posts/shishua-the-fastest-prng-in-the-world.html

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

### Initial benchmarks

The benchmarks measure generation of `double`s.
Iterations = `double`s per op.

There is likely overhead in capturing hardware counters, so these should be more "correct"

NOTE - MWC256 is likely poorly implemented (it is supposed to be faster). As seen in the instrumented benchmark below there are a lot of branch mispredictions.
This is clear from the generated assembly atm but I'm not sure why those branching instructions are generated. `UInt128` support is pretty new
so maybe there are some inefficiencies there.

![Scaling iterations](/img/perf-scaling.png "Scaling iterations")

#### With hardware counters

Instrumented with more diagnostics, including hardware counters

![With hardware counters](/img/perf-hardwarecounters.png "With hardware counters")

