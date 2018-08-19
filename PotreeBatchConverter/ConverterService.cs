using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PotreeBatchConverter
{
    public class ConverterService
    {
        private readonly string _logSeparator;
        private readonly List<TargetSystem> _targetSystems;

        public static ConverterService Create(List<TargetSystem> targetSystem)
        {
            return new ConverterService(targetSystem);
        }

        private ConverterService(List<TargetSystem> targetSystems)
        {
            foreach (var system in targetSystems)
            {
                if (!Enum.IsDefined(typeof(TargetSystem), system))
                    throw new InvalidEnumArgumentException(nameof(system), (int) system, typeof(TargetSystem));
            }

            _targetSystems = targetSystems;
            _logSeparator = GetLogSeparator();
        }

        public void ConvertFilesInDirectoryToTargetSystems(string inputDirectory)
        {
            foreach (var targetSystem in _targetSystems)
                ConvertFilesInDirectoryToTargetSystem(targetSystem, inputDirectory);
        }

        public void ConvertFilesInDirectoryToTargetSystem(TargetSystem targetSystem, string inputDirectory)
        {
            Console.WriteLine(
                $"[Reading all {targetSystem.GetDescription()}-compatible files from {inputDirectory} and its subfolders...]");

            var files = Directory.EnumerateFiles(inputDirectory, "*.*", SearchOption.AllDirectories)
                .Where(file => FileTypeIsSupportedByTargetSystem(targetSystem, file));

            foreach (var file in files)
            {
                ConvertFileToTargetSystem(targetSystem, file);
            }

            WriteLogSeparator();
            Console.WriteLine($"Finished processing folder {inputDirectory}");
        }

        public void ConvertFileToTargetSystems(string file)
        {
            foreach (var targetSystem in _targetSystems)
                ConvertFileToTargetSystem(targetSystem, file);
        }

        public void ConvertFileToTargetSystem(TargetSystem targetSystem, string file)
        {
            WriteLogSeparator();
            Console.WriteLine($"[Processing {file}]");
            Console.WriteLine();

            if (!FileTypeIsSupportedByTargetSystem(targetSystem, file))
            {
                Console.WriteLine($"File type is not supported for {targetSystem} conversion: [{file}]");
                return;
            }

            StartConversionProcess(targetSystem, file);

            Console.WriteLine($"[Done pocessing {file}]");
        }

        private static void StartConversionProcess(TargetSystem targetSystem, string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = GetCommandForTargetSystem(targetSystem, filePath),
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

        private static string GetCommandForTargetSystem(TargetSystem targetSystem, string filePath)
        {
            var currentDirectory = GetCurrentDirectory();

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (targetSystem)
            {
                case TargetSystem.Potree:
                    return
                        $"/c {currentDirectory}\\PotreeConverter\\PotreeConverter.exe \"{filePath}\" -o \"{filePath}.potree\"";
                case TargetSystem.Nexus:
                    return $"/c {currentDirectory}\\Nexus_4.2\\nxsbuild.exe \"{filePath}\" -o \"{filePath}.nxs\"";
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private bool FileTypeIsSupportedByTargetSystem(TargetSystem targetSystem, string file)
        {
            return GetSupportedFileTypes(targetSystem).Any(file.ToLower().EndsWith);
        }

        private IEnumerable<string> GetSupportedFileTypes(TargetSystem targetSystem)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (targetSystem)
            {
                case TargetSystem.Potree:
                    return new[] {".las", ".laz", ".xyz", ".ptx", ".ply"};
                case TargetSystem.Nexus:
                    return new[] {".ply"};
                default:
                    throw new ArgumentException($"No supported file types defined for target system: {_targetSystems}");
            }
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