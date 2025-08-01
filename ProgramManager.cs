using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows;

namespace Launcher
{
    public class ProgramManager
    {
        private readonly string basePath;

        public ProgramManager(string basePath)
        {
            this.basePath = basePath;
        }

        public IEnumerable<(string Name, string Description, string ExePath)> GetInstalledPrograms()
        {
            if (!Directory.Exists(basePath))
                yield break;

            foreach (var dir in Directory.GetDirectories(basePath))
            {
                string shortName = Path.GetFileName(dir);
                string descFile = Path.Combine(dir, "description.txt");
                string description = shortName;
                if (File.Exists(descFile))
                    description = File.ReadAllText(descFile).Trim();

                var exeFiles = Directory.GetFiles(dir, "*.exe")
                    .Where(f => !Path.GetFileName(f).StartsWith("unins", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (exeFiles.Count == 0)
                    continue;

                yield return (shortName, description, exeFiles[0]);
            }
        }

        public void StartProgram(string exePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
    }
}