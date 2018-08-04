using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace PotreeBatchConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello");
            var currentPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var currentDir = new Uri(Path.GetDirectoryName(currentPath));
            Console.WriteLine($"Reading all .las files from {currentDir.LocalPath} and its subfolders...");

            var files = Directory.EnumerateFiles(currentDir.LocalPath, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".las", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith(".laz", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith(".ptx", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith("ply, StringComparison.OrdinalIgnoreCase"));

            foreach (var file in files)
            {
                Console.WriteLine($"Processing {file}");
                var procInfo = new ProcessStartInfo("PotreeConverter.exe ", $"--help")
                {
                    CreateNoWindow = false,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                //--source { file}
                var procRun = Process.Start(procInfo);
                Thread.Sleep(3000);
                procRun.WaitForExit();
                Console.WriteLine($"Done pocessing {file}");
            }

            Console.WriteLine("All done.");
            Console.ReadKey();

        }
    }
}
