using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Launcher
{
    public class DroppedFileManager
    {
        private readonly string droppedFilesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Launcher",
            "droppedfiles.txt"
        );

        public List<(string FilePath, string ButtonLabel)> LoadFiles()
        {
            if (!File.Exists(droppedFilesPath))
                return new List<(string, string)>();

            return File.ReadAllLines(droppedFilesPath)
                .Select(line =>
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                        return (parts[0], parts[1]);
                    else
                        return (parts[0], Path.GetFileName(parts[0])); // fallback: fájlnév mint gombfelirat
                })
                .ToList();
        }

        public void SaveFiles(List<(string FilePath, string ButtonLabel)> files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(droppedFilesPath));
            File.WriteAllLines(droppedFilesPath,
                files.Select(f => $"{f.FilePath}|{f.ButtonLabel}"));
        }

        public void AddFile(string filePath, string buttonLabel = null)
        {
            var files = LoadFiles();
            if (buttonLabel == null)
                buttonLabel = Path.GetFileName(filePath);

            if (!files.Any(f => f.FilePath == filePath))
            {
                files.Add((filePath, buttonLabel));
                SaveFiles(files);
            }
        }

        public void RemoveFile(string filePath)
        {
            var files = LoadFiles()
                .Where(f => f.FilePath != filePath)
                .ToList();
            SaveFiles(files);
        }

        public void OverwriteFiles(List<(string FilePath, string ButtonLabel)> files)
        {
            SaveFiles(files);
        }

        public void ClearFiles()
        {
            if (File.Exists(droppedFilesPath))
                File.Delete(droppedFilesPath);
        }
    }

}