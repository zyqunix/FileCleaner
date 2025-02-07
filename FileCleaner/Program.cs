using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void CleanName(ref string name)
    {
        name = Regex.Replace(name, @"\(\*\)|\[\*\]", "");
        name = Regex.Replace(name, @"\(prod\.[^)]+\)", "", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"\s+", " ").Trim();
        name = Regex.Replace(name, @"\s+(\.mp3)$", "$1");
    }

    static bool EndsWithMp3(string filename)
    {
        return filename.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase);
    }

    static void ProcessDirectory(string path)
    {
        foreach (var directory in Directory.GetDirectories(path))
        {
            ProcessDirectory(directory);
        }

        foreach (var file in Directory.GetFiles(path))
        {
            string fileName = Path.GetFileName(file);
            string newFileName = fileName;
            CleanName(ref newFileName);

            if (EndsWithMp3(newFileName) && !fileName.Equals(newFileName, StringComparison.OrdinalIgnoreCase))
            {
                string newFilePath = Path.Combine(path, newFileName);
                File.Move(file, newFilePath);
            }
        }
    }

    static void Main()
    {
        Console.Write("Enter the directory path: ");
        string path = Console.ReadLine();

        if (Directory.Exists(path))
        {
            ProcessDirectory(path);
            Console.WriteLine("Finished processing, you may close this window");

        }
        else
        {
            Console.WriteLine("Directory does not exist");
        }
    }

}
