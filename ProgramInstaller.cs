using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Launcher
{
    public static class ProgramInstaller
    {
        public static async Task InstallAsync(List<(string ProgramName, string Status)> updateList)
        {
            LoggerService.Info("Starting installation process...");

            foreach (var (programName, status) in updateList)
            {
                // Get installer file name from ProgramsInfo
                if (!ProgramsInfoReader.FetchProgramsInfo(SettingsReader.Settings.VersionPath)
                    .TryGetValue(programName, out var info))
                {
                    LoggerService.Error($"Missing ProgramsInfo for {programName}");
                    continue;
                }

                string installerFileName = info.Installer;
                string localInstallerPath = Path.Combine(Path.GetTempPath(), installerFileName);

                // Special case: Launcher update
                if (info.Executable.Equals("Launcher.exe", StringComparison.OrdinalIgnoreCase))
                {
                    LoggerService.Info("Launcher update detected.");

                    // Notify user to manually install if auto-close is not implemented
                    await MainWindow.Instance._dialogCoordinator.ShowMessageAsync(
                        MainWindow.Instance, "Info", $"Please close all running Launcher instances and run the installer manually:\n{localInstallerPath}");
                    // Optional: Kill running Launcher instances
                    /*foreach (var process in Process.GetProcessesByName("Launcher"))
                    {
                        try { process.Kill(); } catch { /* Ignore errors  }
                    }

                    // Optional: Start installer
                    Process.Start(localInstallerPath);*/
                    continue;
                }

                // Run installer silently or normally
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
                }
                catch (Exception ex)
                {
                    LoggerService.Error($"Failed to start installer for {programName}: {ex.Message}");
                }

                // Wait between installations
                await Task.Delay(1000);
            }

            LoggerService.Info("Installation process completed.");
        }
    }
}
