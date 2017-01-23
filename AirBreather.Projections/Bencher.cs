using System;
using System.Buffers;

using BenchmarkDotNet.Attributes;

using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;

using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems.Transformations;

namespace AirBreather.Projections
{
    public class Bencher
    {
        private const int CNT = 1000000;

        // "EXACT" values (WGS84 is defined by these parameters).
        private const double A_WGS84 = 6_378_137;
        private const double INVERSE_FLATTENING_WGS84 = 298.257_223_563;

        // DERIVED values.
        private const double PI_OVER_4 = Math.PI / 4;
        private const double PI_OVER_180 = Math.PI / 180;
        private const double FLATTENING_WGS84 = 1 / INVERSE_FLATTENING_WGS84;
        private const double LONGITUDE_DEGREES_TO_WGS84 = PI_OVER_180 * A_WGS84;
        ////private const double B_WGS84 = A_WGS84 * (1 - FLATTENING_WGS84);
        private const double ECCENTRICITY_SQUARED_WGS84 = FLATTENING_WGS84 * (2 - FLATTENING_WGS84);
        private static readonly double ECCENTRICITY_WGS84 = Math.Sqrt(ECCENTRICITY_SQUARED_WGS84);
        private static readonly double HALF_ECCENTRICITY_WGS84 = ECCENTRICITY_WGS84 * 0.5;

        private readonly double[] xs = new double[CNT];
        private readonly double[] ys = new double[CNT];

        private readonly double[] outXs = new double[CNT];
        private readonly double[] outYs = new double[CNT];
        private readonly Coordinate[] coords = new Coordinate[CNT];

        private readonly IMathTransform projNetTransform;

        public Bencher()
        {
            Random rand = new Random(12345);
            for (int i = 0; i < CNT; ++i)
            {
                xs[i] = rand.NextDouble() * 360 - 180;
                ys[i] = rand.NextDouble() * 170 - 85;
                coords[i] = new Coordinate(xs[i], ys[i]);
            }

            const string ProjectionWKT = @"PROJCS[""WGS 84 / World Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.01745329251994328,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],PROJECTION[""Mercator_1SP""],PARAMETER[""central_meridian"",0],PARAMETER[""latitude_of_origin"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],AUTHORITY[""EPSG"",""3395""],AXIS[""Easting"",EAST],AXIS[""Northing"",NORTH]]";
            var projCS = (IProjectedCoordinateSystem)CoordinateSystemWktReader.Parse(ProjectionWKT);
            projNetTransform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(projCS.GeographicCoordinateSystem, projCS).MathTransform;
        }

        [Benchmark(Baseline = true)]
        public void ProjectProjNet() => projNetTransform.TransformList(coords);

        [Benchmark]
        public void ProjectScalar()
        {
            for (int offset = 0; offset < xs.Length; ++offset)
            {
                outXs[offset] = xs[offset] * LONGITUDE_DEGREES_TO_WGS84;

                double a = ys[offset] * PI_OVER_180;
                double b = Math.Sin(a);
                double c = b * ECCENTRICITY_WGS84;
                double d = 1 - c;
                double e = 1 + c;
                double f = d / e;
                double g = Math.Pow(f, HALF_ECCENTRICITY_WGS84);
                double h = a / 2;
                double i = h + PI_OVER_4;
                double j = Math.Tan(i);
                double k = j * g;
                double l = Math.Log(k);
                outYs[offset] = l * A_WGS84;
            }
        }

        [Benchmark]
        public void ProjectScalarUnrolled()
        {
            if ((xs.Length & 3) != 0)
            {
                throw new NotSupportedException("I don't feel like dealing with the remainder right now.");
            }

            for (int offset = 4; offset < xs.Length; offset += 4)
            {
                double x1 = xs[offset - 4] * LONGITUDE_DEGREES_TO_WGS84;
                double x2 = xs[offset - 3] * LONGITUDE_DEGREES_TO_WGS84;
                double x3 = xs[offset - 2] * LONGITUDE_DEGREES_TO_WGS84;
                double x4 = xs[offset - 1] * LONGITUDE_DEGREES_TO_WGS84;

                outXs[offset - 4] = x1;
                outXs[offset - 3] = x2;
                outXs[offset - 2] = x3;
                outXs[offset - 1] = x4;

                double a1 = ys[offset - 4] * PI_OVER_180;
                double a2 = ys[offset - 3] * PI_OVER_180;
                double a3 = ys[offset - 2] * PI_OVER_180;
                double a4 = ys[offset - 1] * PI_OVER_180;

                double b1 = Math.Sin(a1);
                double b2 = Math.Sin(a2);
                double b3 = Math.Sin(a3);
                double b4 = Math.Sin(a4);

                double c1 = b1 * ECCENTRICITY_WGS84;
                double c2 = b2 * ECCENTRICITY_WGS84;
                double c3 = b3 * ECCENTRICITY_WGS84;
                double c4 = b4 * ECCENTRICITY_WGS84;

                double d1 = 1 - c1;
                double d2 = 1 - c2;
                double d3 = 1 - c3;
                double d4 = 1 - c4;

                double e1 = 1 + c1;
                double e2 = 1 + c2;
                double e3 = 1 + c3;
                double e4 = 1 + c4;

                double f1 = d1 / e1;
                double f2 = d2 / e2;
                double f3 = d3 / e3;
                double f4 = d4 / e4;

                double g1 = Math.Pow(f1, HALF_ECCENTRICITY_WGS84);
                double g2 = Math.Pow(f2, HALF_ECCENTRICITY_WGS84);
                double g3 = Math.Pow(f3, HALF_ECCENTRICITY_WGS84);
                double g4 = Math.Pow(f4, HALF_ECCENTRICITY_WGS84);

                double h1 = a1 / 2;
                double h2 = a2 / 2;
                double h3 = a3 / 2;
                double h4 = a4 / 2;

                double i1 = h1 + PI_OVER_4;
                double i2 = h2 + PI_OVER_4;
                double i3 = h3 + PI_OVER_4;
                double i4 = h4 + PI_OVER_4;

                double j1 = Math.Tan(i1);
                double j2 = Math.Tan(i2);
                double j3 = Math.Tan(i3);
                double j4 = Math.Tan(i4);

                double k1 = j1 * g1;
                double k2 = j2 * g2;
                double k3 = j3 * g3;
                double k4 = j4 * g4;

                double l1 = Math.Log(k1);
                double l2 = Math.Log(k2);
                double l3 = Math.Log(k3);
                double l4 = Math.Log(k4);

                double m1 = l1 * A_WGS84;
                double m2 = l2 * A_WGS84;
                double m3 = l3 * A_WGS84;
                double m4 = l4 * A_WGS84;

                outYs[offset - 4] = m1;
                outYs[offset - 3] = m2;
                outYs[offset - 2] = m3;
                outYs[offset - 1] = m4;
            }
        }

        [Benchmark] public void ProjectYeppp_1024() => ProjectYeppp(1024);
        [Benchmark] public void ProjectYeppp_2048() => ProjectYeppp(2048);
        [Benchmark] public void ProjectYeppp_4096() => ProjectYeppp(4096);
        [Benchmark] public void ProjectYeppp_8192() => ProjectYeppp(8192);
        [Benchmark] public void ProjectYeppp_16384() => ProjectYeppp(16384);

        private void ProjectYeppp(int maxChunkSize)
        {
            int fullChunkSize = Math.Min(maxChunkSize, CNT);
            int endOfAllFullChunks = (xs.Length / fullChunkSize) * fullChunkSize;

            double[] twoWideScratchBuffer = ArrayPool<double>.Shared.Rent(fullChunkSize << 1);
            try
            {
                for (int i = 0; i < endOfAllFullChunks; i += fullChunkSize)
                {
                    Project(xs, ys, outXs, outYs, twoWideScratchBuffer, i, fullChunkSize);
                }

                if (endOfAllFullChunks != CNT)
                {
                    Project(xs, ys, outXs, outYs, twoWideScratchBuffer, endOfAllFullChunks, CNT - endOfAllFullChunks);
                }
            }
            finally
            {
                ArrayPool<double>.Shared.Return(twoWideScratchBuffer);
            }
        }

        private static void Project(double[] xs, double[] ys, double[] outXs, double[] outYs, double[] twoWideScratchBuffer, int offset, int cnt)
        {
            // outXs[offset] = xs[offset] * LONGITUDE_DEGREES_TO_WGS84;
            Yeppp.Core.Multiply_V64fS64f_V64f(xs, offset, LONGITUDE_DEGREES_TO_WGS84, outXs, offset, cnt);

            // everything else is to deal with the complicated latitude stuff.
            // use 3 "scratch" spaces where we can store intermediate values... kinda like registers
            // if we had true arbitrarily-sized "vector registers" that could hold a full "chunk".
            // 2 "scratch" spaces come from a separate buffer, and the output array is the third.
            double[] scratch1 = outYs;
            int scratch1Off = offset;

            double[] scratch2 = twoWideScratchBuffer;
            int scratch2Off = 0;

            double[] scratch3 = twoWideScratchBuffer;
            int scratch3Off = cnt;

            // "register" allocations:
            // scratch1 gets a, h, i, j, k, l
            // scratch2 gets b, c, e, g
            // scratch3 gets d
            // nobody   gets f (it's transient in the scalar portion)
            //
            // double a = ys[offset] * PI_OVER_180;
            Yeppp.Core.Multiply_V64fS64f_V64f(ys, offset, PI_OVER_180, scratch1, scratch1Off, cnt);

            // double b = Math.Sin(a);
            Yeppp.Math.Sin_V64f_V64f(outYs, offset, scratch2, scratch2Off, cnt);

            // double c = b * ECCENTRICITY_WGS84;
            Yeppp.Core.Multiply_V64fS64f_V64f(scratch2, scratch2Off, ECCENTRICITY_WGS84, scratch2, scratch2Off, cnt);

            // double d = 1 - c;
            Yeppp.Core.Subtract_S64fV64f_V64f(1, scratch2, scratch2Off, scratch3, scratch3Off, cnt);

            // double e = 1 + c;
            Yeppp.Core.Add_IV64fS64f_IV64f(scratch2, scratch2Off, 1, cnt);

            // double f = d / e;
            // double g = Math.Pow(f, HALF_ECCENTRICITY_WGS84);
            // Yeppp! doesn't support division or POW yet.
            for (int i = 0; i < cnt; ++i)
            {
                scratch2[scratch2Off + i] = Math.Pow(scratch3[scratch3Off + i] / scratch2[scratch2Off + i], HALF_ECCENTRICITY_WGS84);
            }

            // double h = a / 2;
            Yeppp.Core.Multiply_V64fS64f_V64f(scratch1, scratch1Off, 0.5, scratch1, scratch1Off, cnt);

            // double i = h + PI_OVER_4;
            Yeppp.Core.Add_V64fS64f_V64f(scratch1, scratch1Off, PI_OVER_4, scratch1, scratch1Off, cnt);

            // double j = Math.Tan(i);
            Yeppp.Math.Tan_V64f_V64f(scratch1, scratch1Off, scratch1, scratch1Off, cnt);

            // double k = j * g;
            Yeppp.Core.Multiply_V64fV64f_V64f(scratch1, scratch1Off, scratch2, scratch2Off, scratch1, scratch1Off, cnt);

            // double l = Math.Log(k);
            Yeppp.Math.Log_V64f_V64f(scratch1, scratch1Off, scratch1, scratch1Off, cnt);

            // outYs[offset] = l * A_WGS84;
            Yeppp.Core.Multiply_V64fS64f_V64f(scratch1, scratch1Off, A_WGS84, outYs, offset, cnt);
        }
    }
}
