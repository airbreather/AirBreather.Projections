#include <immintrin.h>
#include <math.h>
#include <stdint.h>

#define LONGITUDE_DEGREES_TO_WGS84 111319.49079327357
#define PI_OVER_180 0.017453292519943295
#define ECCENTRICITY_WGS84 0.081819190842621486
#define HALF_ECCENTRICITY_WGS84 0.040909595421310743
#define PI_OVER_4 0.78539816339744828
#define A_WGS84 6378137

__declspec(dllexport) void proj_wgs84_scalar(const int cnt, const double* xs, const double* ys, double* outXs, double* outYs)
{
	for (int offset = 0; offset < cnt; offset++)
	{
		double x = xs[offset];
		x = x * LONGITUDE_DEGREES_TO_WGS84;
		outXs[offset] = x;

		double a = ys[offset];
		double b = a * PI_OVER_180;
		double c = sin(b);
		double d = c * ECCENTRICITY_WGS84;
		double e = 1 - d;
		double f = 1 + d;
		double g = e / f;
		double h = pow(g, HALF_ECCENTRICITY_WGS84);
		double i = b / 2;
		double j = i + PI_OVER_4;
		double k = tan(j);
		double l = k * h;
		double m = log(l);
		double n = m * A_WGS84;
		outYs[offset] = n;
	}
}

__declspec(dllexport) void proj_wgs84_scalar_unrolled(const int cnt, const double* xs, const double* ys, double* outXs, double* outYs)
{
	int offset;
	for (offset = 4; offset <= cnt; offset += 4)
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

		double c1 = sin(b1);
		double c2 = sin(b2);
		double c3 = sin(b3);
		double c4 = sin(b4);

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

		double h1 = pow(g1, HALF_ECCENTRICITY_WGS84);
		double h2 = pow(g2, HALF_ECCENTRICITY_WGS84);
		double h3 = pow(g3, HALF_ECCENTRICITY_WGS84);
		double h4 = pow(g4, HALF_ECCENTRICITY_WGS84);

		double i1 = b1 / 2;
		double i2 = b2 / 2;
		double i3 = b3 / 2;
		double i4 = b4 / 2;

		double j1 = i1 + PI_OVER_4;
		double j2 = i2 + PI_OVER_4;
		double j3 = i3 + PI_OVER_4;
		double j4 = i4 + PI_OVER_4;

		double k1 = tan(j1);
		double k2 = tan(j2);
		double k3 = tan(j3);
		double k4 = tan(j4);

		double l1 = k1 * h1;
		double l2 = k2 * h2;
		double l3 = k3 * h3;
		double l4 = k4 * h4;

		double m1 = log(l1);
		double m2 = log(l2);
		double m3 = log(l3);
		double m4 = log(l4);

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
	proj_wgs84_scalar(cnt - offset, xs + offset, ys + offset, outXs + offset, outYs + offset);
}

#define v __m256d
#define v_stride 4
#define v_init(x) _mm256_set1_pd(x)
#define v_mul(x1, x2) _mm256_mul_pd(x1, x2)
#define v_load(x) _mm256_loadu_pd(x)
#define v_store(p, x) _mm256_storeu_pd(p, x)
#define v_sin(x) _mm256_sin_pd(x)
#define v_sub(x1, x2) _mm256_sub_pd(x1, x2)
#define v_add(x1, x2) _mm256_add_pd(x1, x2)
#define v_div(x1, x2) _mm256_div_pd(x1, x2)
#define v_pow(x1, x2) _mm256_pow_pd(x1, x2)
#define v_tan(x) _mm256_tan_pd(x)
#define v_log(x) _mm256_log_pd(x)

__declspec(dllexport) void proj_wgs84_avx2(const int cnt, const double* xs, const double* ys, double* outXs, double* outYs)
{
	/* some constants we use... they're not compiled in as true "constants", so
	   we need to do a little bit of a tango here. */
	static int initted = 0;
	static v one, two, longitude_degrees_to_wgs84, pi_over_180, eccentricity_wgs84, a_wgs84, half_eccentricity_wgs84, pi_over_4;
	if (!initted)
	{
		one = v_init(1);
		two = v_init(2);

		longitude_degrees_to_wgs84 = v_init(LONGITUDE_DEGREES_TO_WGS84);
		pi_over_180 = v_init(PI_OVER_180);
		eccentricity_wgs84 = v_init(ECCENTRICITY_WGS84);
		a_wgs84 = v_init(A_WGS84);
		half_eccentricity_wgs84 = v_init(HALF_ECCENTRICITY_WGS84);
		pi_over_4 = v_init(PI_OVER_4);

		initted = 1;
	}

	/* calculate how far we can go full vectorized */
	const int vec_end = cnt - (cnt % v_stride);

	/* vector loop */
	int offset;
	for (offset = 0; offset < vec_end; offset += v_stride)
	{
		v x = v_load(xs + offset);
		x = v_mul(x, longitude_degrees_to_wgs84);
		v_store(outXs + offset, x);

		v a = v_load(ys + offset);
		v b = v_mul(a, pi_over_180);
		v c = v_sin(b);
		v d = v_mul(c, eccentricity_wgs84);
		v e = v_sub(one, d);
		v f = v_add(one, d);
		v g = v_div(e, f);
		v h = v_pow(g, half_eccentricity_wgs84);
		v i = v_div(b, two);
		v j = v_add(i, pi_over_4);
		v k = v_tan(j);
		v l = v_mul(k, h);
		v m = v_log(l);
		v n = v_mul(m, a_wgs84);
		v_store(outYs + offset, n);
	}

	/* couldn't figure out masked load / store to avoid a scalar loop.
	   oh well, this is quite good enough as it is. */
	proj_wgs84_scalar(cnt - offset, xs + offset, ys + offset, outXs + offset, outYs + offset);
}
