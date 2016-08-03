using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace TestApp
{
    class Program
    {
        private static void Main(string[] args)
        {
            // Run tests
            new Chaskey.Tests.ChaskeyTest().TestBattery();
            
            // JIT + heat up the CPU
            BenchmarkChaskey(2621440, 4 * 1024, false);
            // Real benchmark - digest 4 KiB 2 621 440 times (10 GiB of data)
            BenchmarkChaskey(2621440, 4 * 1024, true);
            // And 15 bytes 71582788 times (1 GiB of data)
            BenchmarkChaskey(71582788, 15, true);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void BenchmarkChaskey(int iterations, int length, bool log)
        {
            // Get specified amount of random data
            var data = GetRandomBytes(length);
            // Initialize Chaskey engine with a random key
            var chaskey = new Chaskey.Chaskey(GetRandomBytes(16));
            var tag = new byte[16];

            // Benchmark
            var stopWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
                chaskey.Compute(data, 0, data.Length, tag, 0);
            var elapsed = stopWatch.Elapsed;

            if (log)
            {
                Console.WriteLine("Chaskey benchmark results:");
                Console.WriteLine("- Digested {0} {1} times", BytesToString(data.Length), iterations);
                Console.WriteLine("- Elapsed: {0}", elapsed.ToString(@"hh\:mm\:ss\.fff"));
                Console.WriteLine("- Speed: {0}/s", BytesToString(data.Length / elapsed.TotalSeconds * iterations));
            }
        }

        private static byte[] GetRandomBytes(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Number of bytes cannot be negative.");

            var bytes = new byte[count];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(bytes);
            return bytes;
        }

        private static string BytesToString(double bytes)
        {
            if (bytes < 1024)
                return bytes.ToString("N0") + " B";

            var KiB = bytes / 1024d;
            if (KiB < 1024)
                return KiB.ToString("N1") + " KiB";

            var MiB = KiB / 1024d;
            return MiB.ToString("N1") + " MiB";
        }
    }
}
