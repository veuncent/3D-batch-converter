using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PotreeBatchConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Potree Batch Converter...");

            var currentPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var currentDir = new Uri(Path.GetDirectoryName(currentPath)).LocalPath;

            Console.WriteLine($"Reading all Potree-compatible files from {currentDir} and its subfolders...");

            var files = Directory.EnumerateFiles(currentDir, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".las", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith(".laz", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith(".ptx", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith("ply, StringComparison.OrdinalIgnoreCase"));

            foreach (var file in files)
            {
                RunProcess(currentDir, file);
            }

            Console.WriteLine("All done. Press the 'Any' key to exit...");
            Console.ReadKey();

        }

        private static void RunProcess(string currentDir, string file)
        {
            Console.WriteLine($"Processing {file}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = $"/c {currentDir}\\PotreeConverter\\PotreeConverter.exe \"{file}\" -o \"{file}.potree\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process { StartInfo = startInfo };

            process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Console.WriteLine($"Done pocessing {file}");
        }
    }
}
