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

### Running tests

```pwsh
dotnet test -c Release --logger:"console;verbosity=detailed"
```

### Initial benchmarks

The benchmarks measure generation of `double`s.
Iterations = `double`s per op.

#### With hardware counters

![With hardware counters](/img/perf-hardwarecounters.png "With hardware counters")

#### Scaling iterations

There is likely overhead in capturing hardware counters, so these should be more "correct"

![Scaling iterations](/img/perf-scaling.png "Scaling iterations")

