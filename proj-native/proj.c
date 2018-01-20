#include <immintrin.h>
#include <math.h>
#include <stdint.h>

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
	while (o < cnt)
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

		o++;
	}
}
