using System;
using System.IO;
using System.Text.RegularExpressions;

namespace LibraryExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "path/to/your/file.txt"; // Replace with the actual file path

            try
            {
                using (StreamReader reader = File.OpenText(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ProcessLine(line);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found. Please provide a valid file path.");
            }
        }

        static void ProcessLine(string line)
        {
            // Regular expressions to match the syntax
            Regex usingLibraryRegex = new Regex(@"using\s+\{\s*(?<libraryName>[^\}]+)\s*\}");
            Regex usingCustomExeRegex = new Regex(@"using\s+\{\s*custom\s+exe\s*:\s*(?<exeFileName>[^\}]+)\s*\}");
            Regex usingExeRegex = new Regex(@"using\s+\{\s*exe\s*:\s*(?<exeFileName>[^\}]+)\s*\}");
            Regex functionNameRegex = new Regex(@"(?<functionName>[^\[]+)\s*\[\s*argument\[(?<argName>[^\]]+)\]\s*:\s*argument\[(?<argValue>[^\]]+)\]\s*\]");

            Match usingLibraryMatch = usingLibraryRegex.Match(line);
            Match usingCustomExeMatch = usingCustomExeRegex.Match(line);
            Match usingExeMatch = usingExeRegex.Match(line);
            Match functionNameMatch = functionNameRegex.Match(line);

            if (usingLibraryMatch.Success)
            {
                string libraryName = usingLibraryMatch.Groups["libraryName"].Value;
                Console.WriteLine($"Library needed: {libraryName}");
            }
            else if (usingCustomExeMatch.Success)
            {
                string exeFileName = usingCustomExeMatch.Groups["exeFileName"].Value;
                Console.WriteLine($"Custom EXE needed: {exeFileName}");
            }
            else if (usingExeMatch.Success)
            {
                string exeFileName = usingExeMatch.Groups["exeFileName"].Value;
                Console.WriteLine($"EXE needed (from application folder): {exeFileName}");
            }
            else if (functionNameMatch.Success)
            {
                string functionName = functionNameMatch.Groups["functionName"].Value;
                string argName = functionNameMatch.Groups["argName"].Value;
                string argValue = functionNameMatch.Groups["argValue"].Value;
                Console.WriteLine($"Function: {functionName}, Argument: {argName}, Value: {argValue}");
            }
        }
    }
}
