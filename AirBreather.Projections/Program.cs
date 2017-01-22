using System;
using System.Diagnostics;

using GeoAPI.Geometries;

namespace AirBreather.Projections
{
    internal static class Program
    {
        private static void Main()
        {
            const int YepppIterations = 20;
            const int ProjNetIterations = 8;
            const int CNT = 10000000;
            double[] xs = new double[CNT];
            double[] ys = new double[CNT];

            Random rand = new Random(12345);
            for (int i = 0; i < CNT; i++)
            {
                xs[i] = rand.NextDouble() * 360 - 180;
                ys[i] = rand.NextDouble() * 170 - 85;
            }

            double[] outXs = new double[CNT];
            double[] outYs = new double[CNT];

            // warm up the static stuff and the JIT.
            WorldMercator.ProjectYeppp(xs, ys, outXs, outYs);

            GC.Collect();
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < YepppIterations; ++i)
            {
                WorldMercator.ProjectYeppp(xs, ys, outXs, outYs);
            }

            sw.Stop();

            Console.WriteLine($"New: {sw.ElapsedTicks / (double)(Stopwatch.Frequency * YepppIterations):N9} seconds.");

            var results = new (double x, double y)[CNT];
            for (int i = 0; i < results.Length; i++)
            {
                results[i].x = outXs[i];
                results[i].y = outYs[i];
            }

            var ins = new Coordinate[CNT];
            for (int i = 0; i < ins.Length; i++)
            {
                ins[i] = new Coordinate(xs[i], ys[i]);
            }

            // warm up the static stuff and the JIT.
            var results2 = WorldMercator.ProjectProjNet(ins);

            GC.Collect();

            sw.Restart();
            for (int i = 0; i < ProjNetIterations; ++i)
            {
                results2 = WorldMercator.ProjectProjNet(ins);
            }

            sw.Stop();

            Console.WriteLine($"Old: {sw.ElapsedTicks / (double)(Stopwatch.Frequency * ProjNetIterations):N9} seconds.");

            for (int i = 0; i < CNT; ++i)
            {
                (double x, double y) expected = (results2[i].X, results2[i].Y);
                (double x, double y) actual = results[i];
                double diff = Math.Sqrt((expected.x - actual.x) * (expected.x - actual.y) + (expected.y - actual.y) * (expected.y - actual.y));
                if (diff > 1)
                {
                    throw new Exception("More than 1 meter of difference between the two methods.");
                }
            }
        }
    }
}