using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace AirBreather.Projections
{
    internal static class Program
    {
        private static void Main()
        {
#if false
            new Bencher().Verify();
#else
            BenchmarkRunner.Run<Bencher>(
                ManualConfig.Create(DefaultConfig.Instance)
                            .With(Job.Default
                                     .WithGcServer(true)));
#endif
        }
    }
}
