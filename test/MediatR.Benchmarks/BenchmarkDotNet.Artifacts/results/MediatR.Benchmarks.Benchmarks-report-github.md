``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18363
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT


```
|                  Method |      Mean |     Error |    StdDev |
|------------------------ |----------:|----------:|----------:|
|         SendingRequests | 23.748 us | 0.8423 us | 2.2915 us |
| PublishingNotifications |  9.289 us | 0.2322 us | 0.6587 us |
