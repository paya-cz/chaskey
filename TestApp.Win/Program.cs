using System;

namespace TestApp
{
    class Program
    {
        private static void Main(string[] args)
        {
            // Run tests
            new Chaskey.Tests.ChaskeyTest().TestBattery();

            // Run benchmarks
            Chaskey.Benchmarks.Benchmark.Run(Console.WriteLine);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
