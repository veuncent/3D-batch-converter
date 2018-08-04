using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PotreeBatchConverter
{
    internal class PotreeConverterService
    {
        private readonly string _currentDirectory;

        public PotreeConverterService()
        {
            _currentDirectory = GetCurrentDirectory();
        }

        public void ConvertFilesInDirectory(string inputDirectory)
        {
            Console.WriteLine($"Reading all Potree-compatible files from {inputDirectory} and its subfolders...");

            var files = Directory.EnumerateFiles(inputDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".las", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith(".laz", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith(".ptx", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith("ply", StringComparison.OrdinalIgnoreCase));

            foreach (var file in files)
            {
                RunProcess(file);
            }

            Console.WriteLine("All done. Press the 'Any' key to exit...");
            Console.ReadKey();
        }

        private void RunProcess(string file)
        {
            Console.WriteLine($"Processing {file}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments =
                    $"/c {_currentDirectory}\\PotreeConverter\\PotreeConverter.exe \"{file}\" -o \"{file}.potree\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process {StartInfo = startInfo};

            process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Console.WriteLine($"Done pocessing {file}");
        }

        private static string GetCurrentDirectory()
        {
            var currentPath = GetCurrentPath();
            var directoryName = Path.GetDirectoryName(currentPath) ?? throw new InvalidOperationException();
            return new Uri(directoryName).LocalPath;
        }

        private static string GetCurrentPath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        }
    }
}