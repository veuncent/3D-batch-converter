using System;

namespace PotreeBatchConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting Potree Batch Converter...");
            Console.WriteLine(Environment.NewLine);

            var converterService = new PotreeConverterService();
            converterService.ConvertFilesInDirectory(inputDir);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("All done. Press the 'Any' key to exit...");
            Console.ReadKey();
        }
    }
}
