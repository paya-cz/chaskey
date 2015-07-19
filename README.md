# Chaskey (C#)
Chaskey PRF implementation in C#. The code is in public domain, free for anyone to use.

[Chaskey](http://mouha.be/chaskey/) is a PRF heavily based on SipHash, but targeting limited 32-bit platforms, like microcontrollers etc. For classic desktop computers, [SipHash](https://131002.net/siphash/) offers better performance even on 32-bit processors. I recommend using [tanglebones](https://github.com/tanglebones/ch-siphash/) C# implementation of SipHash, particularly the [SipHash_2_4_UlongCast_ForcedInline](https://github.com/tanglebones/ch-siphash/blob/master/CH.SipHash/SipHash.cs#L7) variant for maximum performance.
