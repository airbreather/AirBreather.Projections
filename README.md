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
        ProjectProjNet | 197.7892 ms | 0.9480 ms |   1.00 |          0.00 | 168.32 MB |
         ProjectScalar | 103.0010 ms | 0.0788 ms |   0.52 |          0.00 |       0 B |
 ProjectScalarUnrolled | 100.6315 ms | 0.0227 ms |   0.51 |          0.00 |       0 B |
     ProjectYeppp_1024 |  72.5151 ms | 0.0126 ms |   0.37 |          0.00 |       0 B |
     ProjectYeppp_2048 |  72.4519 ms | 0.0166 ms |   0.37 |          0.00 |       0 B |
     ProjectYeppp_4096 |  72.4023 ms | 0.3012 ms |   0.37 |          0.00 |       0 B |
     ProjectYeppp_8192 |  72.5578 ms | 0.0329 ms |   0.37 |          0.00 |       0 B |
    ProjectYeppp_16384 |  72.3359 ms | 0.1798 ms |   0.37 |          0.00 |       0 B |
