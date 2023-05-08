using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Cdocker || beta version";
        Console.ForegroundColor = ConsoleColor.Green;

        while (true)
        {
            Console.Write("Enter command: ");
            var input = Console.ReadLine().Trim();
            if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
            {
                break;
            }
            else if (input.StartsWith("create "))
            {
                var filePath = input.Substring("create ".Length).Trim();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Docker.Create(filePath);
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (input.StartsWith("run "))
            {
                var moduleFilePath = input.Substring("run ".Length).Trim();
                RunModule(moduleFilePath);
            }
            else
            {
                Console.WriteLine("Unknown command");
            }
        }
    }

    private static void RunModule(string moduleFilePath)
    {
        // Check if the file exists
        if (!File.Exists(moduleFilePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: File '{moduleFilePath}' does not exist");
            Console.ForegroundColor = ConsoleColor.Green;
            return;
        }

        // Read the encrypted bytes from the file
        var encryptedBytes = File.ReadAllBytes(moduleFilePath);

        // Decrypt the bytes
        var decryptedBytes = Docker.Decrypt(encryptedBytes);

        // Create a temporary exe file
        var tempExePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".exe");
        File.WriteAllBytes(tempExePath, decryptedBytes);

        // Redirect the output of the exe file to the console
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = tempExePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        Console.ForegroundColor = ConsoleColor.White;
        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        Console.ForegroundColor = ConsoleColor.Green;
        // Delete the temporary exe file
        File.Delete(tempExePath);
    }
}
