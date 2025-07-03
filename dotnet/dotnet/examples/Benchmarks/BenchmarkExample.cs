using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenManus.Examples.Benchmarks
{
    public class BenchmarkExample
    {
        public static async Task RunBenchmarkAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Simulate some work with the Semantic Kernel
            await Task.Delay(1000); // Replace with actual Semantic Kernel calls

            stopwatch.Stop();
            Console.WriteLine($"Benchmark completed in {stopwatch.ElapsedMilliseconds} ms");
        }

        public static void Main(string[] args)
        {
            RunBenchmarkAsync().GetAwaiter().GetResult();
        }
    }
}