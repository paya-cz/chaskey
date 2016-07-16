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

            // JIT
            BenchmarkChaskey(1, 1, false);
            // Real benchmark - digest 4 KiB 2 621 440 times (10 GiB of data)
            BenchmarkChaskey(2621440, 4 * 1024, true);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void BenchmarkChaskey(int iterations, int length, bool log)
        {
            // Get random key
            var key = GetRandomBytes(16);
            // Get specified amount of random data
            var data = GetRandomBytes(length);
            // Initialize Chaskey engine
            var chaskey = new Chaskey.Chaskey(key);
            var tag = new byte[16];

            // Benchmark
            var stopWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
                chaskey.Compute(data, 0, data.Length, tag, 0);
            var elapsed = stopWatch.Elapsed;

            if (log)
            {
                Console.WriteLine("Chaskey benchmark results:");
                Console.WriteLine("- Elapsed: {0}", elapsed.ToString(@"hh\:mm\:ss\.fff"));
                Console.WriteLine("- Digested {0} bytes ({1} KiB) {2} times", data.Length, (data.Length / 1024d).ToString("N2"), iterations);
                Console.WriteLine("- Speed: {0} MiB/s", (data.Length / 1024d / 1024d / elapsed.TotalSeconds * iterations).ToString("N2"));
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
    }
}
