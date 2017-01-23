using BenchmarkDotNet.Running;

namespace AirBreather.Projections
{
    internal static class Program
    {
        private static void Main() => BenchmarkRunner.Run<Bencher>();
    }
}