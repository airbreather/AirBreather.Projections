using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;

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
        private const int CNT = 1024 * 1024 + 3;

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
            LoadLibrary(@"C:\Program Files (x86)\IntelSWTools\compilers_and_libraries_2018.1.156\windows\redist\intel64_win\compiler\svml_dispmd.dll");
            LoadLibrary(@"C:\Users\Joe\src\AirBreather.Projections\x64\Release\proj-native.dll");

            if (!GCSettings.IsServerGC)
            {
                throw new Exception();
            }

            Random rand = new Random(12345);
            for (int i = 0; i < CNT; ++i)
            {
                xs[i] = rand.NextDouble() * 360 - 180;
                ys[i] = rand.NextDouble() * 170 - 85;
                coords[i] = new Coordinate(xs[i], ys[i]);
            }

            const string ProjectionWKT = @"PROJCS[""WGS 84 / World Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.01745329251994328,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],PROJECTION[""Mercator_1SP""],PARAMETER[""central_meridian"",0],PARAMETER[""latitude_of_origin"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],AUTHORITY[""EPSG"",""3395""],AXIS[""Easting"",EAST],AXIS[""Northing"",NORTH]]";
            var projCS = (IProjectedCoordinateSystem)CoordinateSystemWktReader.Parse(ProjectionWKT, Encoding.Default);
            projNetTransform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(projCS.GeographicCoordinateSystem, projCS).MathTransform;

            this.Verify();
        }

        public void Verify()
        {
            var c = projNetTransform.TransformList(coords);
            double[] baselineXs = (double[])this.outXs.Clone();
            double[] baselineYs = (double[])this.outYs.Clone();
            for (int i = 0; i < CNT; ++i)
            {
                (baselineXs[i], baselineYs[i]) = (c[i].X, c[i].Y);
            }

            this.ProjectScalar();
            VerifyNext(nameof(this.ProjectScalar));
            this.ProjectScalarUnrolled();
            VerifyNext(nameof(this.ProjectScalarUnrolled));
            this.ProjectNative_AVX2();
            VerifyNext(nameof(this.ProjectNative_AVX2));
            this.ProjectNative_Scalar();
            VerifyNext(nameof(this.ProjectNative_Scalar));
            this.ProjectNative_ScalarUnrolled();
            VerifyNext(nameof(this.ProjectNative_ScalarUnrolled));

            void VerifyNext(string alg)
            {
                for (int i = 0; i < CNT; ++i)
                {
                    const double MaxDelta = 0.00000001;
                    if (Math.Abs(baselineXs[i] - this.outXs[i]) > MaxDelta)
                    {
                        throw new Exception($"{alg}: X{i} too far apart: << {baselineXs[i]} >> vs. << {this.outXs[i]} >>");
                    }

                    if (Math.Abs(baselineYs[i] - this.outYs[i]) > MaxDelta)
                    {
                        throw new Exception($"{alg}: Y{i} too far apart: << {baselineYs[i]} >> vs. << {this.outYs[i]} >>");
                    }
                }
            }
        }

        [Benchmark(Baseline = true)]
        public void ProjectProjNet() => projNetTransform.TransformList(coords);

        [Benchmark]
        public void ProjectScalar()
        {
            for (int offset = 0; offset < xs.Length; ++offset)
            {
                double x = xs[offset];
                x = xs[offset] * LONGITUDE_DEGREES_TO_WGS84;
                outXs[offset] = x;

                double a = ys[offset];
                double b = a * PI_OVER_180;
                double c = Math.Sin(b);
                double d = c * ECCENTRICITY_WGS84;
                double e = 1 - d;
                double f = 1 + d;
                double g = e / f;
                double h = Math.Pow(g, HALF_ECCENTRICITY_WGS84);
                double i = b / 2;
                double j = i + PI_OVER_4;
                double k = Math.Tan(j);
                double l = k * h;
                double m = Math.Log(l);
                double n = m * A_WGS84;
                outYs[offset] = n;
            }
        }

        [Benchmark]
        public void ProjectScalarUnrolled()
        {
            int offset;
            for (offset = 4; offset <= xs.Length; offset += 4)
            {
                double x1 = xs[offset - 4];
                double x2 = xs[offset - 3];
                double x3 = xs[offset - 2];
                double x4 = xs[offset - 1];

                x1 = x1 * LONGITUDE_DEGREES_TO_WGS84;
                x2 = x2 * LONGITUDE_DEGREES_TO_WGS84;
                x3 = x3 * LONGITUDE_DEGREES_TO_WGS84;
                x4 = x4 * LONGITUDE_DEGREES_TO_WGS84;

                outXs[offset - 4] = x1;
                outXs[offset - 3] = x2;
                outXs[offset - 2] = x3;
                outXs[offset - 1] = x4;

                double a1 = ys[offset - 4];
                double a2 = ys[offset - 3];
                double a3 = ys[offset - 2];
                double a4 = ys[offset - 1];

                double b1 = a1 * PI_OVER_180;
                double b2 = a2 * PI_OVER_180;
                double b3 = a3 * PI_OVER_180;
                double b4 = a4 * PI_OVER_180;

                double c1 = Math.Sin(b1);
                double c2 = Math.Sin(b2);
                double c3 = Math.Sin(b3);
                double c4 = Math.Sin(b4);

                double d1 = c1 * ECCENTRICITY_WGS84;
                double d2 = c2 * ECCENTRICITY_WGS84;
                double d3 = c3 * ECCENTRICITY_WGS84;
                double d4 = c4 * ECCENTRICITY_WGS84;

                double e1 = 1 - d1;
                double e2 = 1 - d2;
                double e3 = 1 - d3;
                double e4 = 1 - d4;

                double f1 = 1 + d1;
                double f2 = 1 + d2;
                double f3 = 1 + d3;
                double f4 = 1 + d4;

                double g1 = e1 / f1;
                double g2 = e2 / f2;
                double g3 = e3 / f3;
                double g4 = e4 / f4;

                double h1 = Math.Pow(g1, HALF_ECCENTRICITY_WGS84);
                double h2 = Math.Pow(g2, HALF_ECCENTRICITY_WGS84);
                double h3 = Math.Pow(g3, HALF_ECCENTRICITY_WGS84);
                double h4 = Math.Pow(g4, HALF_ECCENTRICITY_WGS84);

                double i1 = b1 / 2;
                double i2 = b2 / 2;
                double i3 = b3 / 2;
                double i4 = b4 / 2;

                double j1 = i1 + PI_OVER_4;
                double j2 = i2 + PI_OVER_4;
                double j3 = i3 + PI_OVER_4;
                double j4 = i4 + PI_OVER_4;

                double k1 = Math.Tan(j1);
                double k2 = Math.Tan(j2);
                double k3 = Math.Tan(j3);
                double k4 = Math.Tan(j4);

                double l1 = k1 * h1;
                double l2 = k2 * h2;
                double l3 = k3 * h3;
                double l4 = k4 * h4;

                double m1 = Math.Log(l1);
                double m2 = Math.Log(l2);
                double m3 = Math.Log(l3);
                double m4 = Math.Log(l4);

                double n1 = m1 * A_WGS84;
                double n2 = m2 * A_WGS84;
                double n3 = m3 * A_WGS84;
                double n4 = m4 * A_WGS84;

                outYs[offset - 4] = n1;
                outYs[offset - 3] = n2;
                outYs[offset - 2] = n3;
                outYs[offset - 1] = n4;
            }

            offset -= 4;
            for (; offset < xs.Length; ++offset)
            {
                double x = xs[offset];
                x = x * LONGITUDE_DEGREES_TO_WGS84;
                outXs[offset] = x;

                double a = ys[offset];
                double b = a * PI_OVER_180;
                double c = Math.Sin(b);
                double d = c * ECCENTRICITY_WGS84;
                double e = 1 - d;
                double f = 1 + d;
                double g = e / f;
                double h = Math.Pow(g, HALF_ECCENTRICITY_WGS84);
                double i = b / 2;
                double j = i + PI_OVER_4;
                double k = Math.Tan(j);
                double l = k * h;
                double m = Math.Log(l);
                double n = m * A_WGS84;
                outYs[offset] = n;
            }
        }

        [Benchmark]
        public void ProjectNative_Scalar() => proj_wgs84_scalar(CNT, xs, ys, outXs, outYs);

        [Benchmark]
        public void ProjectNative_ScalarUnrolled() => proj_wgs84_scalar_unrolled(CNT, xs, ys, outXs, outYs);

        [Benchmark]
        public void ProjectNative_AVX2() => proj_wgs84_avx2(CNT, xs, ys, outXs, outYs);

        [DllImport("proj-native.dll")]
        private static extern void proj_wgs84_scalar(int cnt, double[] xs, double[] ys, double[] outXs, double[] outYs);

        [DllImport("proj-native.dll")]
        private static extern void proj_wgs84_scalar_unrolled(int cnt, double[] xs, double[] ys, double[] outXs, double[] outYs);

        [DllImport("proj-native.dll")]
        private static extern void proj_wgs84_avx2(int cnt, double[] xs, double[] ys, double[] outXs, double[] outYs);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);
    }
}
