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
|        ProjectProjNet | 210.63 ms | 0.4428 ms | 0.3697 ms |   1.00 |
|         ProjectScalar | 109.09 ms | 0.0479 ms | 0.0425 ms |   0.52 |
| ProjectScalarUnrolled | 106.08 ms | 0.0552 ms | 0.0516 ms |   0.50 |
|     ProjectYeppp_1024 |  76.19 ms | 0.0642 ms | 0.0601 ms |   0.36 |
|     ProjectYeppp_2048 |  75.78 ms | 0.0786 ms | 0.0656 ms |   0.36 |
|     ProjectYeppp_4096 |  75.87 ms | 0.0467 ms | 0.0414 ms |   0.36 |
|     ProjectYeppp_8192 |  76.01 ms | 0.1878 ms | 0.1665 ms |   0.36 |
|    ProjectYeppp_16384 |  76.06 ms | 0.1078 ms | 0.0956 ms |   0.36 |
|         ProjectNative |  26.49 ms | 0.0431 ms | 0.0382 ms |   0.13 |
