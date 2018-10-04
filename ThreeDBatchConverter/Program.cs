using System;
using System.Collections.Generic;
using System.IO;

namespace ThreeDBatchConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting Potree Batch Converter...");
            Console.WriteLine(Environment.NewLine);

            var targetSystems = AskTargetSystems();
            var converterService = ConverterService.Create(targetSystems);

            var fileOrDirectory = AskFileOrDirectory();
            string inputPath;
            switch (fileOrDirectory)
            {
                case "f":
                case "file":
                    inputPath = AskInputFile();
                    converterService.ConvertFileToTargetSystems(inputPath);
                    break;
                case "d":
                case "directory":
                    inputPath = AskInputDirectory();
                    converterService.ConvertFilesInDirectoryToTargetSystems(inputPath);
                    break;
                default:
                    throw new ArgumentException();
            }


            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("All done. Press the 'Any' key to exit...");
            Console.ReadKey();
        }

        private static List<TargetSystem> AskTargetSystems()
        {
            Console.WriteLine("For which systems would you like to convert?");
            var targetSystems = new List<TargetSystem>();

            var isValidInput = false;
            do
            {
                Console.WriteLine("Valid options are:");
                Console.WriteLine("     'potree' (or 'p')");
                Console.WriteLine("     'nexus' (or 'n') (this is 3d hop)");
                Console.WriteLine("     'both' ( or 'b')");
                var input = Console.ReadLine();
                switch (input)
                {
                    case "potree":
                    case "p":
                        targetSystems.Add(TargetSystem.Potree);
                        isValidInput = true;
                        break;
                    case "nexus":
                    case "n":
                        targetSystems.Add(TargetSystem.Nexus);
                        isValidInput = true;
                        break;
                    case "both":
                    case "b":
                        targetSystems.Add(TargetSystem.Potree);
                        targetSystems.Add(TargetSystem.Nexus);
                        isValidInput = true;
                        break;
                    default:
                        Console.WriteLine($"Invalid input: {input}");
                        break;
                }
            } while (!isValidInput);

            return targetSystems;
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