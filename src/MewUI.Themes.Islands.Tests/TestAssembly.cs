using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// MewUI controls share process-wide visual-tree state and must run on one test thread at a time.
[assembly: Parallelization(Mode = ParallelMode.None)]
