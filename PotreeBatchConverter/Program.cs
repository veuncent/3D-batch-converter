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

            var converterService = new PotreeConverterService();

            var fileOrDirectory = AskFileOrDirectory();

            switch (fileOrDirectory)
            {
                case "f":
                case "file":
                    var inputFile = AskInputFile();
                    converterService.ConvertFileToPotree(inputFile);
                    break;
                case "d":
                case "directory":
                    var inputDir = AskInputDirectory();
                    converterService.ConvertFilesInDirectory(inputDir);
                    break;
                default:
                    throw new ArgumentException();
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("All done. Press the 'Any' key to exit...");
            Console.ReadKey();
        }


        private static string AskFileOrDirectory()
        {
            Console.WriteLine("Do you want to process a single file or a directory?");
            string fileOrDirectory;
            var isValidAnswer = false;

            do
            {
                Console.WriteLine("Please choose between: file (f) or directory (d)");
                fileOrDirectory = Console.ReadLine();
                if (fileOrDirectory != "file" &
                    fileOrDirectory != "f" &
                    fileOrDirectory != "directory" &
                    fileOrDirectory != "d")
                    Console.WriteLine("Invalid choice");
                else
                    isValidAnswer = true;
            } while (!isValidAnswer);

            return fileOrDirectory;
        }

        private static string AskInputFile()
        {
            Console.WriteLine("Which file do you want to process?");

            var isValidFile = false;
            var inputFile = "";

            while (!isValidFile)
            {
                inputFile = Console.ReadLine();
                if (File.Exists(inputFile))
                    isValidFile = true;
                else
                {
                    Console.WriteLine("File does not exist, please try again");
                }
            }

            return inputFile;
        }

        private static string AskInputDirectory()
        {
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

            return inputDir;
        }
    }
}