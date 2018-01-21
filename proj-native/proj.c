#include <immintrin.h>
#include <math.h>
#include <stdint.h>

__declspec(dllexport) void proj_wgs84_scalar(const int cnt, const double* xs, const double* ys, double* outXs, double* outYs)
{
	for (int o = 0; o < cnt; o++)
	{
		outXs[o] = xs[o] * 111319.49079327357;

		double a = ys[o] * 0.017453292519943295;
		double b = sin(a);
		double c = b * 0.081819190842621486;
		double d = 1 - c;
		double e = 1 + c;
		double f = d / e;
		double g = pow(f, 0.040909595421310743);
		double h = a / 2;
		double i = h + 0.78539816339744828;
		double j = tan(i);
		double k = j * g;
		double l = log(k);
		outYs[o] = l * 6378137;
	}
}

__declspec(dllexport) void proj_wgs84_scalar_unrolled(const int cnt, const double* xs, const double* ys, double* outXs, double* outYs)
{
	int o;
	for (o = 4; o <= cnt; o += 4)
	{
		double x1 = xs[o - 4] * 111319.49079327357;
		double x2 = xs[o - 3] * 111319.49079327357;
		double x3 = xs[o - 2] * 111319.49079327357;
		double x4 = xs[o - 1] * 111319.49079327357;

		outXs[o - 4] = x1;
		outXs[o - 3] = x2;
		outXs[o - 2] = x3;
		outXs[o - 1] = x4;

		double a1 = ys[o - 4] * 0.017453292519943295;
		double a2 = ys[o - 3] * 0.017453292519943295;
		double a3 = ys[o - 2] * 0.017453292519943295;
		double a4 = ys[o - 1] * 0.017453292519943295;

		double b1 = sin(a1);
		double b2 = sin(a2);
		double b3 = sin(a3);
		double b4 = sin(a4);

		double c1 = b1 * 0.081819190842621486;
		double c2 = b2 * 0.081819190842621486;
		double c3 = b3 * 0.081819190842621486;
		double c4 = b4 * 0.081819190842621486;

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

		double g1 = pow(f1, 0.040909595421310743);
		double g2 = pow(f2, 0.040909595421310743);
		double g3 = pow(f3, 0.040909595421310743);
		double g4 = pow(f4, 0.040909595421310743);

		double h1 = a1 / 2;
		double h2 = a2 / 2;
		double h3 = a3 / 2;
		double h4 = a4 / 2;

		double i1 = h1 + 0.78539816339744828;
		double i2 = h2 + 0.78539816339744828;
		double i3 = h3 + 0.78539816339744828;
		double i4 = h4 + 0.78539816339744828;

		double j1 = tan(i1);
		double j2 = tan(i2);
		double j3 = tan(i3);
		double j4 = tan(i4);

		double k1 = j1 * g1;
		double k2 = j2 * g2;
		double k3 = j3 * g3;
		double k4 = j4 * g4;

		double l1 = log(k1);
		double l2 = log(k2);
		double l3 = log(k3);
		double l4 = log(k4);

		double m1 = l1 * 6378137;
		double m2 = l2 * 6378137;
		double m3 = l3 * 6378137;
		double m4 = l4 * 6378137;

		outYs[o - 4] = m1;
		outYs[o - 3] = m2;
		outYs[o - 2] = m3;
		outYs[o - 1] = m4;
	}

	proj_wgs84_scalar(cnt - o, xs + o, ys + o, outXs + o, outYs + o);
}

__declspec(dllexport) void proj_wgs84_avx2(const int cnt, const double* xs, const double* ys, double* outXs, double* outYs)
{
	/* some constants we use... they're not compiled in as true "constants", so
	   we need to do a little bit of a tango here. */
	static int initted = 0;
	static __m256d one, two, xMul, yMul1, yMul2, yMul3, yPow, yAdd;
	if (!initted)
	{
		one = _mm256_set1_pd(1);
		two = _mm256_set1_pd(2);
		xMul = _mm256_set1_pd(111319.49079327357);
		yMul1 = _mm256_set1_pd(0.017453292519943295);
		yMul2 = _mm256_set1_pd(0.081819190842621486);
		yMul3 = _mm256_set1_pd(6378137);
		yPow = _mm256_set1_pd(0.040909595421310743);
		yAdd = _mm256_set1_pd(0.78539816339744828);
		initted = 1;
	}

	/* calculate how far we can go full vectorized */
	const int vec_end = cnt - (cnt % 4);

	/* vector loop */
	int o;
	for (o = 0; o < vec_end; o += 4)
	{
		_mm256_storeu_pd(outXs + o, _mm256_mul_pd(_mm256_loadu_pd(xs + o), xMul));

		__m256d a = _mm256_mul_pd(_mm256_loadu_pd(ys + o), yMul1);
		__m256d b = _mm256_sin_pd(a);
		__m256d c = _mm256_mul_pd(b, yMul2);
		__m256d d = _mm256_sub_pd(one, c);
		__m256d e = _mm256_add_pd(one, c);
		__m256d f = _mm256_div_pd(d, e);
		__m256d g = _mm256_pow_pd(f, yPow);
		__m256d h = _mm256_div_pd(a, two);
		__m256d i = _mm256_add_pd(h, yAdd);
		__m256d j = _mm256_tan_pd(i);
		__m256d k = _mm256_mul_pd(j, g);
		__m256d l = _mm256_log_pd(k);
		_mm256_storeu_pd(outYs + o, _mm256_mul_pd(l, yMul3));
	}

	/* couldn't figure out masked load / store to avoid a scalar loop.
	   oh well, this is quite good enough as it is. */
	proj_wgs84_scalar(cnt - o, xs + o, ys + o, outXs + o, outYs + o);
}
