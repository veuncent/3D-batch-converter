using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace ThreeDBatchConverter
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
                    throw new InvalidEnumArgumentException(nameof(system), (int)system, typeof(TargetSystem));
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
                .Where(filePath => FileTypeIsSupportedByTargetSystem(targetSystem, filePath));

            foreach (var filePath in files)
            {
                ConvertFileToTargetSystem(targetSystem, filePath);
            }

            WriteLogSeparator();
            Console.WriteLine($"Finished processing folder {inputDirectory}");
        }

        public void ConvertFileToTargetSystems(string filePath)
        {
            foreach (var targetSystem in _targetSystems)
                ConvertFileToTargetSystem(targetSystem, filePath);
        }

        public void ConvertFileToTargetSystem(TargetSystem targetSystem, string filePath)
        {
            WriteLogSeparator();
            Console.WriteLine($"[Processing {filePath}]");
            Console.WriteLine();

            if (!FileTypeIsSupportedByTargetSystem(targetSystem, filePath))
            {
                Console.WriteLine($"File type is not supported for {targetSystem} conversion: [{filePath}]");
                return;
            }

            StartConversionProcess(targetSystem, filePath);

            StartPostProcessing(targetSystem, filePath);

            Console.WriteLine($"[Done pocessing {filePath}]");
        }

        private void StartConversionProcess(TargetSystem targetSystem, string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = GetCommandForTargetSystem(targetSystem, filePath),
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
        }

        private void StartPostProcessing(TargetSystem targetSystem, string inputFilePath)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (targetSystem)
            {
                case TargetSystem.Potree:
                    ZipOutputFiles(targetSystem, inputFilePath);
                    break;
                case TargetSystem.Nexus:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetSystem));
            }
        }

        private string GetCommandForTargetSystem(TargetSystem targetSystem, string filePath)
        {
            var currentDirectory = GetCurrentDirectory();
            var outputPath = ResolveOutputPath(targetSystem, filePath);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (targetSystem)
            {
                case TargetSystem.Potree:
                    return
                        $"/c {currentDirectory}\\PotreeConverter\\PotreeConverter.exe \"{filePath}\" -o \"{outputPath}\"";
                case TargetSystem.Nexus:
                    return $"/c {currentDirectory}\\Nexus_4.2\\nxsbuild.exe \"{filePath}\" -o \"{outputPath}\"";
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

        private bool FileTypeIsSupportedByTargetSystem(TargetSystem targetSystem, string filePath)
        {
            return GetSupportedFileTypes(targetSystem).Any(filePath.ToLower().EndsWith);
        }

        private IEnumerable<string> GetSupportedFileTypes(TargetSystem targetSystem)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (targetSystem)
            {
                case TargetSystem.Potree:
                    return new[] { ".las", ".laz", ".xyz", ".ptx", ".ply" };
                case TargetSystem.Nexus:
                    return new[] { ".ply" };
                default:
                    throw new ArgumentException($"No supported originalFilePath types defined for target system: {_targetSystems}");
            }
        }

        private string ResolveOutputPath(TargetSystem targetSystem, string inputPath)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (targetSystem)
            {
                case TargetSystem.Potree:
                    return $"{ResolvePathWithoutExtension(inputPath)}.potree";
                case TargetSystem.Nexus:
                    return $"{ResolvePathWithoutExtension(inputPath)}.nxs";
                default:
                    throw new ArgumentException($"No supported originalFilePath types defined for target system: {_targetSystems}");
            }
        }

        private static string ResolvePathWithoutExtension(string inputPath)
        {
            var directory = Path.GetDirectoryName(inputPath) ?? throw new ArgumentNullException(nameof(inputPath));
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputPath) ?? throw new ArgumentNullException(nameof(inputPath));
            return Path.Combine(directory, fileNameWithoutExtension);
        }

        private void ZipOutputFiles(TargetSystem targetSystem, string originalFilePath)
        {
            var folderToZip = ResolveOutputPath(targetSystem, originalFilePath);

            if (!Directory.Exists(folderToZip))
            {
                Console.WriteLine($"Output folder [{folderToZip}] does not exist. Something went wrong during conversion.");
                return;
            }

            var zipfilePath = $"{folderToZip}.zip";

            if (File.Exists(zipfilePath))
            {
                Console.WriteLine($"Zip file already exists: {zipfilePath}");
                Console.WriteLine("Skipping...");
                return;
            }

            ZipFile.CreateFromDirectory(folderToZip, zipfilePath, CompressionLevel.Optimal, false);

            Console.WriteLine($"Created zipfile: {zipfilePath}");
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