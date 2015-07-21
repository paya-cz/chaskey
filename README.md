# Chaskey (C#)
Chaskey PRF implementation in C#. The code is in public domain, free for anyone to use.

[Chaskey](http://mouha.be/chaskey/) is a PRF heavily based on SipHash, but targeting limited 32-bit platforms, like microcontrollers etc. For classic desktop computers (Intel/AMD), [SipHash](https://131002.net/siphash/) offers better performance even on 32-bit processors. I recommend using [tanglebones](https://github.com/tanglebones/ch-siphash/) C# implementation of SipHash, particularly the [SipHash_2_4_UlongCast_ForcedInline](https://github.com/tanglebones/ch-siphash/blob/master/CH.SipHash/SipHash.cs#L7) variant for maximum performance.

People using [Go](https://golang.org/) might be interested in [Chaskey in Go](https://github.com/dgryski/go-chaskey).

# Optimizations #
If you want maximum performance on modern Intel x86/x64 CPUs, you should manually inline the permutation function and unroll the permutation loop (compiler neither JIT won't do the job, even if you use MethodImplOptions.AggressiveInlining). This gives 2-4 speedup. SipHash still outperforms Chaskey in both 32-bit and 64-bit mode though, even after these optimizations (at least on i7 3630QM).

These are the speeds I benchmarked:

64-bit mode (hashing 10 GiB):
- SipHash: 1 187 MiB/s
- Chaskey: 603 MiB/s

32-bit mode (again 10 GiB):
- SipHash: 329 MiB/s
- Chaskey: 293 MiB/s
