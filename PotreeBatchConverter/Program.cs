using System;
using DotNet.Config;

namespace PotreeBatchConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting Potree Batch Converter...");
            Console.WriteLine(Environment.NewLine);

            var settings = InitializeSettings();
            var inputDir = settings.InputDirectory;
            var converterService = new PotreeConverterService();
            converterService.ConvertFilesInDirectory(inputDir);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("All done. Press the 'Any' key to exit...");
            Console.ReadKey();
        }

        private static Settings InitializeSettings()
        {
            var settings = new Settings();
            AppSettings.GlueOnto(settings, "config.txt");
            return settings;
        }
    }
}
