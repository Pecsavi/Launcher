using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Launcher
{
    public static class ProgramInstaller
    {

        public static async Task InstallAsync(List<(string ProgramName, string Status)> updateList, MetroWindow dialogOwner)
        {
            LoggerService.Info("Starting installation process...");

            // Ensure Launcher is installed last to avoid self-update conflicts
            updateList = updateList
                .OrderBy(u => u.ProgramName.Equals("Launcher", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ToList();


            foreach (var (programName, status) in updateList)
            {
                if (!ProgramsInfoReader.FetchProgramsInfo(SettingsReader.Settings.VersionPath)
                    .TryGetValue(programName, out var info))
                {
                    LoggerService.Error($"Missing ProgramsInfo for {programName}");
                    continue;
                }

                string installerFileName = info.Installer;
                string localInstallerPath = Path.Combine(Path.GetTempPath(), installerFileName);

                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = localInstallerPath,
                            UseShellExecute = true
                        }
                    };
                    process.Start();
                    LoggerService.Info($"Started installer for {programName}");
                    await process.WaitForExitAsync();
                }
                catch (Exception ex)
                {
                    LoggerService.Error($"Failed to start installer for {programName}: {ex.Message}");
                }

                await Task.Delay(1000);
            }

            LoggerService.Info("Installation process completed.");
        }


    }
}
