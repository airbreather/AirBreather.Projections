# AirBreather.Projections

``` ini

BenchmarkDotNet=v0.10.1, OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-6850K CPU 3.60GHz, ProcessorCount=12
Frequency=3515628 Hz, Resolution=284.4442 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1586.0
  DefaultJob : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1586.0


```
                Method |        Mean |    StdDev | Scaled | Scaled-StdDev | Allocated |
---------------------- |------------ |---------- |------- |-------------- |---------- |
        ProjectProjNet | 198.4269 ms | 1.2398 ms |   1.00 |          0.00 | 168.28 MB |
         ProjectScalar | 103.1095 ms | 0.0315 ms |   0.52 |          0.00 |       0 B |
 ProjectScalarUnrolled | 101.0993 ms | 0.4264 ms |   0.51 |          0.00 |       0 B |
      ProjectYeppp_128 |  75.6329 ms | 0.0610 ms |   0.38 |          0.00 |       0 B |
      ProjectYeppp_256 |  73.8275 ms | 0.0578 ms |   0.37 |          0.00 |       0 B |
      ProjectYeppp_512 |  73.6055 ms | 0.6997 ms |   0.37 |          0.00 |       0 B |
     ProjectYeppp_1024 |  72.6718 ms | 0.0387 ms |   0.37 |          0.00 |       0 B |
     ProjectYeppp_2048 |  72.7503 ms | 0.4676 ms |   0.37 |          0.00 |       0 B |
     ProjectYeppp_4096 |  72.2446 ms | 0.0203 ms |   0.36 |          0.00 |       0 B |
     ProjectYeppp_8192 |  72.3205 ms | 0.0255 ms |   0.36 |          0.00 |       0 B |
    ProjectYeppp_16384 |  73.2400 ms | 0.9006 ms |   0.37 |          0.00 |       0 B |
    ProjectYeppp_32768 |  72.5538 ms | 0.0427 ms |   0.37 |          0.00 |       0 B |
    ProjectYeppp_65536 |  72.7582 ms | 0.0567 ms |   0.37 |          0.00 |       0 B |
   ProjectYeppp_131072 |  72.9457 ms | 0.1162 ms |   0.37 |          0.00 |       0 B |
