using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Launcher
{
    public class DroppedFileManager
    {
        private readonly string droppedFilesPath = "droppedfiles.txt";

        public List<string> LoadFiles()
        {
            if (!File.Exists(droppedFilesPath))
                return new List<string>();
            return File.ReadAllLines(droppedFilesPath).ToList();
        }

        public void SaveFiles(List<string> files)
        {
            File.WriteAllLines(droppedFilesPath, files);
        }

        public void AddFile(string file)
        {
            var files = LoadFiles();
            if (!files.Contains(file))
            {
                files.Add(file);
                SaveFiles(files);
            }
        }

        public void RemoveFile(string file)
        {
            var files = LoadFiles();
            if (files.Contains(file))
            {
                files.Remove(file);
                SaveFiles(files);
            }
        }

        public void OverwriteFiles(List<string> files)
        {
            File.WriteAllLines(droppedFilesPath, files);
        }
        public void ClearFiles()
        {
            if (File.Exists(droppedFilesPath))
            {
                File.Delete(droppedFilesPath);
            }
        }
    }
}