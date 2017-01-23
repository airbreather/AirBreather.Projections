using System;
using System.Buffers;
using System.Collections.Generic;

using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;

using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems.Transformations;

namespace AirBreather.Projections
{
    internal static class WorldMercator
    {
        private static readonly IMathTransform ProjNetTransform;

        static WorldMercator()
        {
            const string ProjectionWKT = @"PROJCS[""WGS 84 / World Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.01745329251994328,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],PROJECTION[""Mercator_1SP""],PARAMETER[""central_meridian"",0],PARAMETER[""latitude_of_origin"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],AUTHORITY[""EPSG"",""3395""],AXIS[""Easting"",EAST],AXIS[""Northing"",NORTH]]";
            var projCS = (IProjectedCoordinateSystem)CoordinateSystemWktReader.Parse(ProjectionWKT);
            ProjNetTransform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(projCS.GeographicCoordinateSystem, projCS).MathTransform;
        }

        internal static IList<Coordinate> ProjectProjNet(IList<Coordinate> coordinates) => ProjNetTransform.TransformList(coordinates);

        // "EXACT" values (WGS84 is defined by these parameters).
        private const double A_WGS84 = 6_378_137;
        private const double INVERSE_FLATTENING_WGS84 = 298.257_223_563;

        // DERIVED values.
        private const double PI_OVER_4 = Math.PI / 4;
        private const double PI_OVER_180 = Math.PI / 180;
        private const double FLATTENING_WGS84 = 1 / INVERSE_FLATTENING_WGS84;
        ////private const double B_WGS84 = A_WGS84 * (1 - FLATTENING_WGS84);
        private const double ECCENTRICITY_SQUARED_WGS84 = FLATTENING_WGS84 * (2 - FLATTENING_WGS84);
        private static readonly double ECCENTRICITY_WGS84 = Math.Sqrt(ECCENTRICITY_SQUARED_WGS84);
        private static readonly double HALF_ECCENTRICITY_WGS84 = ECCENTRICITY_WGS84 * 0.5;

        internal static void ProjectYeppp(double[] xs, double[] ys, double[] outXs, double[] outYs)
        {
            // we want to have this be highly "chunky" for a few reasons: every Yeppp! method call
            // does some parameter validation that we know will succeed, and more importantly we
            // really want to minimize stalls between one logical "instruction" and the next one
            // that depends on it... the larger the chunk that we process at once, the smaller the
            // impact that each stall has on the operation.  at the same time, we don't want to just
            // make a single call, or else Yeppp! will push all the intermediate values out of the
            // CPU caches as it scans the remainder of the arrays (not to mention that our scratch
            // buffer size will have to be bigger and bigger).  I've tested some chunk sizes on my
            // monster gaming machine, and it seemed to be about right at 16384, but I'm backing off
            // a bit so that it'll work better on CPUs with smaller cache sizes... which also
            // happens to bring each array under the LOH threshold, though that's not all that
            // relevant because we use a buffer pool to amortize the allocation cost.
            const int ChunkSize = 4096;
            int fullChunkSize = Math.Min(ChunkSize, xs.Length);
            int endOfAllFullChunks = (xs.Length / fullChunkSize) * fullChunkSize;

            double[] twoWideScratchBuffer = ArrayPool<double>.Shared.Rent(fullChunkSize << 1);
            try
            {
                for (int i = 0; i < endOfAllFullChunks; i += fullChunkSize)
                {
                    Project(xs, ys, outXs, outYs, twoWideScratchBuffer, i, fullChunkSize);
                }

                if (endOfAllFullChunks != xs.Length)
                {
                    Project(xs, ys, outXs, outYs, twoWideScratchBuffer, endOfAllFullChunks, xs.Length - endOfAllFullChunks);
                }
            }
            finally
            {
                ArrayPool<double>.Shared.Return(twoWideScratchBuffer);
            }
        }

        private static void Project(double[] xs, double[] ys, double[] outXs, double[] outYs, double[] twoWideScratchBuffer, int offset, int cnt)
        {
            // longitude is easy.
            Yeppp.Core.Multiply_V64fS64f_V64f(xs, offset, PI_OVER_180 * A_WGS84, outXs, offset, cnt);

            // everything else is to deal with the complicated latitude stuff.
            // use 2 "scratch" spaces where we can store intermediate values... kinda like registers
            // if we had true arbitrarily-sized "vector registers" that could hold a full "chunk".
            // we can also use the output array as a third "register".
            double[] scratch1 = outYs;
            int scratch1Off = offset;

            double[] scratch2 = twoWideScratchBuffer;
            int scratch2Off = 0;

            double[] scratch3 = twoWideScratchBuffer;
            int scratch3Off = cnt;

            // "register" allocations:
            // scratch1 gets a, h, i, j, k, l
            // scratch2 gets b, c, e, f/g
            // scratch3 gets d
            //
            // a = DEGREES_TO_RADIANS(y)
            Yeppp.Core.Multiply_V64fS64f_V64f(ys, offset, PI_OVER_180, scratch1, scratch1Off, cnt);

            // b = SIN(a)
            Yeppp.Math.Sin_V64f_V64f(outYs, offset, scratch2, scratch2Off, cnt);

            // c = b * ECCENTRICITY_WGS84
            Yeppp.Core.Multiply_V64fS64f_V64f(scratch2, scratch2Off, ECCENTRICITY_WGS84, scratch2, scratch2Off, cnt);

            // d = (1 - c)
            Yeppp.Core.Subtract_S64fV64f_V64f(1, scratch2, scratch2Off, scratch3, scratch3Off, cnt);

            // e = (1 + c)
            Yeppp.Core.Add_IV64fS64f_IV64f(scratch2, scratch2Off, 1, cnt);

            // f = d / e
            // g = POW(f, HALF_ECCENTRICITY_WGS84)
            // Yeppp! doesn't support division or POW yet.
            for (int i = 0; i < cnt; ++i)
            {
                scratch2[scratch2Off + i] = Math.Pow(scratch3[scratch3Off + i] / scratch2[scratch2Off + i], HALF_ECCENTRICITY_WGS84);
            }

            // h = a / 2
            Yeppp.Core.Multiply_V64fS64f_V64f(scratch1, scratch1Off, 0.5, scratch1, scratch1Off, cnt);

            // i = h + PI_OVER_4
            Yeppp.Core.Add_V64fS64f_V64f(scratch1, scratch1Off, PI_OVER_4, scratch1, scratch1Off, cnt);

            // j = TAN(i)
            Yeppp.Math.Tan_V64f_V64f(scratch1, scratch1Off, scratch1, scratch1Off, cnt);

            // k = j * g
            Yeppp.Core.Multiply_V64fV64f_V64f(scratch1, scratch1Off, scratch2, scratch2Off, scratch1, scratch1Off, cnt);

            // l = LOG(k)
            Yeppp.Math.Log_V64f_V64f(scratch1, scratch1Off, scratch1, scratch1Off, cnt);

            // m = l * A_WGS84
            Yeppp.Core.Multiply_V64fS64f_V64f(scratch1, scratch1Off, A_WGS84, outYs, offset, cnt);
        }
    }
}
