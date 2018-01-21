# AirBreather.Projections

``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.192)
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical cores and 6 physical cores
Frequency=3515627 Hz, Resolution=284.4443 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host]     : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  Job-ZQIYOK : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT

Server=True  

```
|                       Method |      Mean |     Error |    StdDev | Scaled |
|----------------------------- |----------:|----------:|----------:|-------:|
|               ProjectProjNet | 162.46 ms | 0.8711 ms | 0.7722 ms |   1.00 |
|                ProjectScalar |  79.49 ms | 0.0383 ms | 0.0358 ms |   0.49 |
|        ProjectScalarUnrolled |  65.41 ms | 0.0247 ms | 0.0231 ms |   0.40 |
|         ProjectNative_Scalar |  57.59 ms | 0.0376 ms | 0.0351 ms |   0.35 |
| ProjectNative_ScalarUnrolled |  43.34 ms | 0.0620 ms | 0.0550 ms |   0.27 |
|           ProjectNative_AVX2 |  26.86 ms | 0.0184 ms | 0.0163 ms |   0.17 |
