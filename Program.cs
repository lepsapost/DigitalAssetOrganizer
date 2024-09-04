using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter the directory path to scan: ");
        string directory = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(directory))
        {
            Console.WriteLine("No directory path provided.");
            return;
        }

        if (Directory.Exists(directory))
        {
            GenerateReport(directory);
        }
        else
        {
            Console.WriteLine("Invalid directory path.");
        }
    }

    static void GenerateReport(string directory)
    {
        var fileTypes = new Dictionary<string, List<string>>();
        var largeFiles = new List<(string, long)>();
        var duplicates = new Dictionary<string, List<string>>();

        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(file).ToLower();
            if (!fileTypes.ContainsKey(extension))
                fileTypes[extension] = new List<string>();
            fileTypes[extension].Add(file);

            long fileSize = new FileInfo(file).Length;
            if (fileSize > 100 * 1024 * 1024) // Files larger than 100MB
                largeFiles.Add((file, fileSize));

            string fileHash = GetFileHash(file);
            if (!duplicates.ContainsKey(fileHash))
                duplicates[fileHash] = new List<string>();
            duplicates[fileHash].Add(file);
        }

        Console.WriteLine("File Type Summary:");
        foreach (var type in fileTypes)
        {
            Console.WriteLine($"{type.Key}: {type.Value.Count} files");
        }

        Console.WriteLine("\nLarge Files (>100MB):");
        foreach (var (file, size) in largeFiles)
        {
            Console.WriteLine($"{file}: {size / (1024.0 * 1024.0):F2} MB");
        }

        Console.WriteLine("\nPotential Duplicates:");
        foreach (var dup in duplicates.Where(d => d.Value.Count > 1))
        {
            Console.WriteLine($"Duplicate set: {string.Join(", ", dup.Value)}");
        }

        Console.WriteLine("\nSuggested Folder Structure:");
        Console.WriteLine("- Documents");
        Console.WriteLine("- Images");
        Console.WriteLine("- Videos");
        Console.WriteLine("- Music");
        Console.WriteLine("- Archives");
        Console.WriteLine("- Miscellaneous");

        Console.WriteLine("\nDo you want to organize files into the suggested folder structure? (Y/N)");
        if (Console.ReadLine()?.Trim().ToUpper() == "Y")
        {
            OrganizeFiles(directory, fileTypes);
        }
    }

    static string GetFileHash(string filepath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filepath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    static void OrganizeFiles(string sourceDirectory, Dictionary<string, List<string>> fileTypes)
    {
        string baseDirectory = Path.Combine(sourceDirectory, "Organized_Files");
        Directory.CreateDirectory(baseDirectory);

        Dictionary<string, string> categoryMap = new Dictionary<string, string>
        {
            {".doc", "Documents"}, {".docx", "Documents"}, {".pdf", "Documents"}, {".txt", "Documents"},
            {".jpg", "Images"}, {".jpeg", "Images"}, {".png", "Images"}, {".gif", "Images"},
            {".mp4", "Videos"}, {".avi", "Videos"}, {".mov", "Videos"},
            {".mp3", "Music"}, {".wav", "Music"}, {".flac", "Music"},
            {".zip", "Archives"}, {".rar", "Archives"}, {".7z", "Archives"}
        };

        foreach (var fileType in fileTypes)
        {
            string extension = fileType.Key;
            string category = categoryMap.ContainsKey(extension) ? categoryMap[extension] : "Miscellaneous";
            string categoryPath = Path.Combine(baseDirectory, category);
            Directory.CreateDirectory(categoryPath);

            foreach (string filePath in fileType.Value)
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(categoryPath, fileName);
                try
                {
                    File.Move(filePath, destPath);
                    Console.WriteLine($"Moved: {filePath} -> {destPath}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error moving {filePath}: {ex.Message}");
                }
            }
        }

        Console.WriteLine("File organization complete.");
    }
}
