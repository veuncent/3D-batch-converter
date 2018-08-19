using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PotreeBatchConverter
{
    public class ConverterService
    {
        private readonly string _logSeparator;
        private readonly string[] _supportedFileTypes;
        private readonly TargetSystem _targetSystem;

        public static ConverterService Create(TargetSystem targetSystem)
        {
            return new ConverterService(targetSystem);
        }

        private ConverterService(TargetSystem targetSystem)
        {
            if (!Enum.IsDefined(typeof(TargetSystem), targetSystem))
                throw new InvalidEnumArgumentException(nameof(targetSystem), (int) targetSystem, typeof(TargetSystem));

            _targetSystem = targetSystem;
            _logSeparator = GetLogSeparator();
            _supportedFileTypes = GetSupportedFileTypes();
        }

        public void ConvertFilesInDirectory(string inputDirectory)
        {
            Console.WriteLine(
                $"[Reading all {_targetSystem.GetDescription()}-compatible files from {inputDirectory} and its subfolders...]");

            var files = Directory.EnumerateFiles(inputDirectory, "*.*", SearchOption.AllDirectories)
                .Where(FileTypeIsSupportedByTargetSystem);

            foreach (var file in files)
            {
                ConvertFileToTargetSystem(file);
            }

            WriteLogSeparator();
            Console.WriteLine($"Finished processing folder {inputDirectory}");
        }

        public void ConvertFileToTargetSystem(string file)
        {
            WriteLogSeparator();
            Console.WriteLine($"[Processing {file}]");
            Console.WriteLine();

            if (!FileTypeIsSupportedByTargetSystem(file))
            {
                Console.WriteLine($"File type is not supported by {_targetSystem}: [{file}]");
                return;
            }

            StartConversionProcess(file);

            Console.WriteLine($"[Done pocessing {file}]");
        }

        private bool FileTypeIsSupportedByTargetSystem(string file)
        {
            return _supportedFileTypes.Any(file.ToLower().EndsWith);
        }

        private void StartConversionProcess(string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = GetCommandForTargetSystem(filePath),
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

        private string GetCommandForTargetSystem(string filePath)
        {
            var currentDirectory = GetCurrentDirectory();

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_targetSystem)
            {
                case TargetSystem.Potree:
                    return
                        $"/c {currentDirectory}\\PotreeConverter\\PotreeConverter.exe \"{filePath}\" -o \"{filePath}.potree\"";
                case TargetSystem.Nexus:
                    var outputFilePath = GetOutputFilePathForNexus(filePath);
                    return $"/c {currentDirectory}\\Nexus_4.2\\nxsbuild.exe \"{filePath}\" -o \"{outputFilePath}\"";
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

        private static string GetOutputFilePathForNexus(string filePath)
        {
            var fileDirectory = Path.GetDirectoryName(filePath);
            if (fileDirectory == null)
                throw new ArgumentException($"Could not find directory for input file: [{filePath}]");
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var outputFilePath = Path.Combine(fileDirectory, $"{fileNameWithoutExtension}.nxs");
            return outputFilePath;
        }

        private string[] GetSupportedFileTypes()
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_targetSystem)
            {
                case TargetSystem.Potree:
                    return new[] {".las", ".laz", ".xyz", ".ptx", ".ply"};
                case TargetSystem.Nexus:
                    return new[] {".ply"};
                default:
                    throw new ArgumentException($"No supported file types defined for target system: {_targetSystem}");
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