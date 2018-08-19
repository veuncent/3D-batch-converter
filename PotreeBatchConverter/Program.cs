using System;
using System.IO;

namespace PotreeBatchConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting Potree Batch Converter...");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("In what folder should we look for input files?");

            var isValidFolder = false;
            var inputDir = "";

            while (!isValidFolder)
            {
                inputDir = Console.ReadLine();
                if (Directory.Exists(inputDir))
                    isValidFolder = true;
                else
                {
                    Console.WriteLine("Folder does not exist, please try again");
                }
            }

            var converterService = new PotreeConverterService();
            converterService.ConvertFilesInDirectory(inputDir);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("All done. Press the 'Any' key to exit...");
            Console.ReadKey();
        }
    }
}
