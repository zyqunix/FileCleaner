using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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

        foreach (var file in Directory.GetFiles(path, "*.mp3"))
        {
            string fileName = Path.GetFileName(file);
            string newFileName = fileName;
            CleanName(ref newFileName);

            string newFilePath = file;
            if (!fileName.Equals(newFileName, StringComparison.OrdinalIgnoreCase))
            {
                newFilePath = Path.Combine(path, newFileName);
                if (!System.IO.File.Exists(newFilePath))
                {
                    System.IO.File.Move(file, newFilePath);
                }
            }

            UpdateMp3Metadata(newFilePath, path);
        }
    }

    static void UpdateMp3Metadata(string filePath, string directoryPath)
    {
        try
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            using (var tagFile = TagLib.File.Create(filePath))
            {
                tagFile.Tag.Title = fileNameWithoutExtension;

                int year = GetAlbumReleaseYear(directoryPath);
                if (year != 0)
                {
                    tagFile.Tag.Year = (uint)year;
                }

                string album = Path.GetFileName(directoryPath);
                if (!string.IsNullOrWhiteSpace(album))
                {
                    tagFile.Tag.Album = album;
                }

                string artist = new DirectoryInfo(directoryPath).Parent?.Name;
                // string artist = "<artist>";

                /* change this to the actual artist name or
                 * make this get the artist name from the directory path of the album
                 * e.g:
                 *
                 * <artist> -> <album/s>        -> <songs>
                 * woody    -> My Favorite Kill -> Made It Out.mp3
                 * 
                 * ^^^^^^^^ THIS WAS JUST CHANGED ^^^^^^^^
                 */
                if (!string.IsNullOrWhiteSpace(artist))
                {
                    tagFile.Tag.Performers = new string[] { artist };
                    tagFile.Tag.AlbumArtists = new string[] { artist };
                }

                tagFile.Save();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating {filePath}: {ex.Message}");
        }
    }

    static int GetAlbumReleaseYear(string directoryPath)
    {
        var albumYears = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
            { "Dear Diary", 2024 },
            { "1night2plugs", 2024 },
            { "smile in her sleep", 2024 },
            { "DollHouse", 2023 },
            { "Fish", 2023 },
            { "Book of Wood", 2023 },
            { "Primitive tech", 2022 },
            { "WOODEN LAKE", 2022 },
            { "My Favorite Kill", 2022 },
            { "Ticks n Pollen", 2022 },
            { "Fetch", 2022 },
            { "woody", 2022 },
            { "burb", 2021 },
            { "Yes, Pleasevol.3", 2021 }
        };

        string albumName = new DirectoryInfo(directoryPath).Name;
        return albumYears.TryGetValue(albumName, out int year) ? year : 0;
    }

    static void Main()
    {
        Console.Write("Enter the directory path: ");
        string path = Console.ReadLine();

        if (Directory.Exists(path))
        {
            ProcessDirectory(path);
            Console.WriteLine("Finished processing.");
        }
        else
        {
            Console.WriteLine("Directory does not exist.");
        }
    }
}
