using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PotreeBatchConverter
{
    internal class PotreeConverterService
    {
        private readonly string _logSeparator;
        private readonly string[] _potreeSupportedFileTypes = { ".las", ".laz", ".xyz", ".ptx", ".ply" };

        public PotreeConverterService()
        {
            _logSeparator = GetLogSeparator();
        }

        public void ConvertFilesInDirectory(string inputDirectory)
        {
            Console.WriteLine($"[Reading all Potree-compatible files from {inputDirectory} and its subfolders...]");

            var files = Directory.EnumerateFiles(inputDirectory, "*.*", SearchOption.AllDirectories)
                .Where(FileTypeIsSupportedByPotree);

            foreach (var file in files)
            {
                ConvertFileToPotree(file);
            }

            WriteLogSeparator();
            Console.WriteLine($"Finished processing folder {inputDirectory}");
        }

        public void ConvertFileToPotree(string file)
        {
            WriteLogSeparator();
            Console.WriteLine($"[Processing {file}]");
            Console.WriteLine();

            if (!FileTypeIsSupportedByPotree(file))
            {
                Console.WriteLine($"File type is not supported by Potree: [{file}]");
                return;
            }

            StartPotreeProcess(file);

            Console.WriteLine($"[Done pocessing {file}]");
        }

        private bool FileTypeIsSupportedByPotree(string file)
        {
            return _potreeSupportedFileTypes.Any(file.ToLower().EndsWith);
        }

        private static void StartPotreeProcess(string file)
        {
            var currentDirectory = GetCurrentDirectory();

            var startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments =
                    $"/c {currentDirectory}\\PotreeConverter\\PotreeConverter.exe \"{file}\" -o \"{file}.potree\"",
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

        private static string GetLogSeparator()
        {
            var sep = "";
            for (var i = 0; i < Console.WindowWidth; i++)
                sep += "=";
            return sep;
        }

        private void WriteLogSeparator()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(_logSeparator);
        }
    }
}