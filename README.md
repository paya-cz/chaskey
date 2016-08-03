# Chaskey (C#)
Chaskey PRF implementation in C#. The code is in public domain, free for anyone to use.

[Chaskey](http://mouha.be/chaskey/) is a PRF heavily based on [SipHash](https://131002.net/siphash/), but targeting limited 32-bit platforms, like microcontrollers etc. 

# Optimizations #
The C# code has been heavily optimized both for short and long messages.

# Benchmarks #
Below are the results of a benchmark I ran on i7 3630QM against heavily optimized C# SipHash implementation.

64-bit mode (processing 10 GiB in 4 KiB chunks):
- SipHash: 1 226 MiB/s
- Chaskey: 630 MiB/s

32-bit mode (processing 10 GiB in 4 KiB chunks):
- SipHash: 324 MiB/s
- Chaskey: 575 MiB/s