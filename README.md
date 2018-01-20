# AirBreather.Projections

``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.192)
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical cores and 6 physical cores
Frequency=3515627 Hz, Resolution=284.4443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2600.0
  Job-FRGPGK : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2600.0

AllowVeryLargeObjects=True  Server=True  

```
|                Method |      Mean |     Error |    StdDev | Scaled |
|---------------------- |----------:|----------:|----------:|-------:|
|        ProjectProjNet | 210.30 ms | 1.3040 ms | 1.2198 ms |   1.00 |
|         ProjectScalar | 109.19 ms | 0.0234 ms | 0.0195 ms |   0.52 |
| ProjectScalarUnrolled | 105.80 ms | 0.0287 ms | 0.0224 ms |   0.50 |
|     ProjectYeppp_1024 |  76.33 ms | 0.0507 ms | 0.0424 ms |   0.36 |
|     ProjectYeppp_2048 |  75.60 ms | 0.0145 ms | 0.0113 ms |   0.36 |
|     ProjectYeppp_4096 |  75.98 ms | 0.0863 ms | 0.0721 ms |   0.36 |
|     ProjectYeppp_8192 |  76.38 ms | 0.5203 ms | 0.4867 ms |   0.36 |
|    ProjectYeppp_16384 |  76.51 ms | 0.4288 ms | 0.3801 ms |   0.36 |
|  ProjectNative_Scalar |  57.51 ms | 0.3940 ms | 0.3492 ms |   0.27 |
|    ProjectNative_AVX2 |  26.75 ms | 0.0381 ms | 0.0297 ms |   0.13 |
