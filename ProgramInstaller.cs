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

        public static async Task InstallAsync(List<(string ProgramName, string Status)> updateList)
        {
            LoggerService.Info("Starting installation process...");

            bool launcherSelected = updateList.Any(u =>
                ProgramsInfoReader.FetchProgramsInfo(SettingsReader.Settings.VersionPath)
                .TryGetValue(u.ProgramName, out var info) &&
                info.Executable.Equals("Launcher.exe", StringComparison.OrdinalIgnoreCase));

            if (launcherSelected)
            {
                string installerPath = Path.Combine(Path.GetTempPath(),
                    ProgramsInfoReader.FetchProgramsInfo(SettingsReader.Settings.VersionPath)["Launcher"].Installer);

                var result = await MainWindow.Instance._dialogCoordinator.ShowMessageAsync(
                    MainWindow.Instance,
                    "Launcher-Update",
                    $"Der Launcher kann nicht automatisch aktualisiert werden, da er gerade ausgeführt wird.\n" +
                    $"Die Installationsdatei wurde erfolgreich heruntergeladen:\n{installerPath}\n\n" +
                    "Möchten Sie den Launcher jetzt beenden und die Installation manuell starten,\n" +
                    "oder zuerst die anderen Programme installieren?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "Jetzt beenden und installieren",
                        NegativeButtonText = "Andere Programme zuerst installieren"
                    });

                if (result == MessageDialogResult.Affirmative)
                {
                    LoggerService.Info("User chose to exit and install Launcher manually.");
                    MainWindow.Instance.Close(); // bezárja az ablakot, kilépéshez
                    return;
                }
                else
                {
                    LoggerService.Info("User chose to install other programs first.");
                }
            }

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
