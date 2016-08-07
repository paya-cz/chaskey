using System;
using System.Diagnostics;
using System.Globalization;

namespace Chaskey.Benchmarks
{
    public static class Benchmark
    {
        private static readonly Random RNG = new Random();

        public static void Run(Action<string> writeLine)
        {
            // JIT + heat up the CPU
            BenchmarkChaskey(2621440, 4 * 1024, null);
            // Real benchmark - digest 4 KiB 2 621 440 times (10 GiB of data)
            BenchmarkChaskey(2621440, 4 * 1024, writeLine);
            // And 7 bytes 153391689 times (1 GiB of data)
            BenchmarkChaskey(153391689, 7, writeLine);
        }

        private static void BenchmarkChaskey(int iterations, int length, Action<string> writeLine)
        {
            // Initialize SipHash engine with a random key
            var chaskey = new Chaskey(GetRandomBytes(16));
            var tag = new byte[16];

            // Generate specified amount of random data
            var data = GetRandomBytes(length);

            // Benchmark
            var stopWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
                chaskey.Compute(data, 0, data.Length, tag, 0);
            var elapsed = stopWatch.Elapsed;

            if (writeLine != null)
            {
                writeLine("Chaskey benchmark results:");
                writeLine(string.Format(CultureInfo.CurrentUICulture, "- Digested {0} {1} times", BytesToString(data.Length), iterations));
                writeLine(string.Format(CultureInfo.CurrentUICulture, "- Elapsed: {0}", elapsed.ToString(@"hh\:mm\:ss\.fff")));
                writeLine(string.Format(CultureInfo.CurrentUICulture, "- Speed: {0}/s", BytesToString(data.Length / elapsed.TotalSeconds * iterations)));
            }
        }

        private static byte[] GetRandomBytes(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Number of bytes cannot be negative.");

            var bytes = new byte[count];
            RNG.NextBytes(bytes);
            return bytes;
        }

        private static string BytesToString(double bytes)
        {
            if (bytes < 1024)
                return bytes.ToString("N0", CultureInfo.CurrentUICulture) + " B";

            var KiB = bytes / 1024d;
            if (KiB < 1024)
                return KiB.ToString("N1", CultureInfo.CurrentUICulture) + " KiB";

            var MiB = KiB / 1024d;
            return MiB.ToString("N1", CultureInfo.CurrentUICulture) + " MiB";
        }
    }
}
