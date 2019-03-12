# AirBreather.Projections

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical and 6 physical cores
Frequency=3515621 Hz, Resolution=284.4448 ns, Timer=TSC
.NET Core SDK=2.2.200
  [Host]     : .NET Core 2.2.2 (CoreCLR 4.6.27317.07, CoreFX 4.6.27318.02), 64bit RyuJIT
  Job-BPBQCB : .NET Core 2.2.2 (CoreCLR 4.6.27317.07, CoreFX 4.6.27318.02), 64bit RyuJIT

Server=True  

```
|                       Method |      Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|----------------------------- |----------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
|               ProjectProjNet | 176.43 ms | 3.5102 ms | 3.6048 ms |  1.00 |           - |           - |           - |         176161368 B |
|                ProjectScalar |  83.71 ms | 0.1502 ms | 0.1405 ms |  0.47 |           - |           - |           - |                   - |
|        ProjectScalarUnrolled |  63.47 ms | 0.1280 ms | 0.1134 ms |  0.36 |           - |           - |           - |                   - |
|         ProjectNative_Scalar |  60.65 ms | 0.1189 ms | 0.1112 ms |  0.34 |           - |           - |           - |                   - |
| ProjectNative_ScalarUnrolled |  45.06 ms | 0.1013 ms | 0.0947 ms |  0.25 |           - |           - |           - |                   - |
|           ProjectNative_AVX2 |  28.10 ms | 0.0383 ms | 0.0320 ms |  0.16 |           - |           - |           - |                   - |
