using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;


namespace Launcher
{
    public class ProgramUpdater
    {
        private static readonly HttpClient client = new HttpClient();
        private string versionUrl;
        public static Dictionary<string, ProgramsInfo> ServerVersions { get; set; }


        private readonly object _context;
        private readonly IDialogCoordinator dialogCoordinator;

      


        public ProgramUpdater(IDialogCoordinator dialogCoordinator, object context)
        {
            this.dialogCoordinator = dialogCoordinator;
            _context = context;
        }

        public static bool TryGetProgramInfo(string programName, out ProgramsInfo info)
        {
            if (ServerVersions != null && ServerVersions.TryGetValue(programName, out info))
                return true;

            info = null;
            return false;
        }

        public async Task CheckForUpdates()
        {
            try
            {
                var versionInfo = await VersionService.GetVersionInfoAsync();
                foreach (var program in versionInfo.Programs)
                {
                    if (!ProgramIsInstalled(program))
                    {

                        var install = await dialogCoordinator.ShowMessageAsync(
                            _context,
                            "Neues Programm",
                            $"Neues Programm „{program.FolderName}“ ist auf dem Server gefunden.\nMöchtest du installieren?",
                            MessageDialogStyle.AffirmativeAndNegative);

                        if (install == MessageDialogResult.Affirmative)
                        {
                            await InstallProgramAsync(program);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await dialogCoordinator.ShowMessageAsync(_context, "Fehler", $"Updateprüfung fehlgeschlagen:\n{ex.Message}");
            }
        }
        public static async Task CheckForNewProgramsAsync(string versionJsonUrl, string localBasePath)
        {
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(versionJsonUrl);

            var serverPrograms = JsonConvert.DeserializeObject<Dictionary<string, ProgramsInfo>>(json);

            if (serverPrograms == null)
                return;

            var localFolders = Directory.GetDirectories(localBasePath)
                                        .Select(Path.GetFileName)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in serverPrograms)
            {
                var programId = kvp.Key;
                var info = kvp.Value;

                if (!localFolders.Contains(info.FolderName))
                {
                    var result = await ShowInstallPromptAsync(info.FolderName);

                    if (result == true)
                    {
                        await InstallProgramAsync(info);
                        LoggerService.UserActivity($"Installed {info.FolderName}");
                    }
                }
            }
        }

        private static async Task<bool> ShowInstallPromptAsync(string programName)
        {
            var dialogResult = await DialogCoordinator.Instance.ShowMessageAsync(
                MainWindow.Instance, 
                "Neues Programm verfügbar",
                $"Das Programm \"{programName}\" ist nicht installiert.\nMöchten Sie es installieren?",
                MessageDialogStyle.AffirmativeAndNegative);

            return dialogResult == MessageDialogResult.Affirmative;
        }

        public static async Task InstallProgramAsync(ProgramsInfo info)
        {
            var tempInstallerPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(info.Installer));

            using var httpClient = new HttpClient();
            var installerBytes = await httpClient.GetByteArrayAsync(info.Installer);
            await File.WriteAllBytesAsync(tempInstallerPath, installerBytes);

            // Digitális aláírás ellenőrzés (ha van ilyen funkció)
            if (!DigitalSignatureVerifier.IsValid(tempInstallerPath))
            {

                LoggerService.Info($"Installer for {info.FolderName} is not signed. Installation aborted.");
                await DialogCoordinator.Instance.ShowMessageAsync(
                    MainWindow.Instance,
                    "Sicherheitsprüfung",
                    $"Die digitale Signatur der Installationsdatei für „{info.FolderName}“ ist ungültig.\nDie Installation wird abgebrochen.");
                return;

            }

            Process.Start(new ProcessStartInfo
            {
                FileName = tempInstallerPath,
                UseShellExecute = true
            });
        }


        /// <summary>
        /// Checks for updates for a specific program and installs if necessary.
        /// </summary>
        public async Task<bool> CheckAndUpdateAsync(string programName, string exePath)
        {
            versionUrl = ConfigurationProvider.Settings.VersionPath;
            if (string.IsNullOrEmpty(versionUrl))
                return true;

            string json = await FetchVersionJsonAsync(versionUrl);
            if (string.IsNullOrEmpty(json))
            {
                LoggerService.Info($"Versionsprüfung übersprungen für {programName}: Keine programsInfo.json verfügbar.");
                return true;
            }

            ServerVersions = JsonConvert.DeserializeObject<Dictionary<string, ProgramsInfo>>(json);

            if (ServerVersions == null || !ServerVersions.TryGetValue(programName, out ProgramsInfo info))
            {
                LoggerService.Info($"Programm {programName} wurde in der programsInfo.json nicht gefunden.");
                return true;
            }

            return await CompareVersionsAsync(programName, exePath, info);
        }




        /// <summary>
        /// Downloads the programsInfo.json content from the server.
        /// </summary>
        private async Task<string> FetchVersionJsonAsync(string url)
        {
            try
            {
                return await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, $"Die Datei programsInfo.json konnte von der URL {url} nicht heruntergeladen werden.");
                return null;
            }
        }

        /// <summary>
        /// Parses the programsInfo.json content into a dictionary.
        /// </summary>
        private bool TryExtractServerVersions(string json)
        {
            try
            {
                
                ServerVersions = JsonConvert.DeserializeObject<Dictionary<string, ProgramsInfo>>(json);
                return ServerVersions != null;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Die Datei programsInfo.json konnte nicht verarbeitet werden.");
                return false;
            }
        }

        /// <summary>
        /// Compares local and server versions and installs update if needed.
        /// </summary>
        private async Task<bool> CompareVersionsAsync(string programName, string exePath, ProgramsInfo info)
        {
            try
            {
                string localVersion = FileVersionInfo.GetVersionInfo(exePath).ProductVersion;
                string serverVersion = info.Version;

                if (localVersion != serverVersion)
                {
                    LoggerService.Info($"Aktualisierung erforderlich für {programName}: lokal = {localVersion}, server = {serverVersion}");


                    var result = await dialogCoordinator.ShowMessageAsync(
                        _context,
                        "Update verfügbar",
                        $"Eine neue Version von {programName} ist verfügbar ({localVersion} → {serverVersion}).\nMöchten Sie sie jetzt installieren?",
                        MessageDialogStyle.AffirmativeAndNegative);


                    if (result == MessageDialogResult.Affirmative)
                    {
                        LoggerService.Info($"Benutzer hat das Update für {programName} akzeptiert.");
                        string installerPath = await DownloadInstallerAsync(info.Installer);

                        if (!string.IsNullOrEmpty(installerPath))
                        {
                            await RunInstaller(installerPath);
                        }

                        return false; // Exit after update
                    }
                }

                return true; // No update or user declined
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, $"Fehler beim Vergleich der Versionen für {programName}.");
                return true;
            }
        }

        /// <summary>
        /// Downloads the installer from the given URL.
        /// </summary>
        private async Task<string> DownloadInstallerAsync(string url)
        {
            try
            {
                LoggerService.Info($"Installationsprogramm wird heruntergeladen: {url}");
                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(url));
                var data = await client.GetByteArrayAsync(url);
                File.WriteAllBytes(tempPath, data);
                return tempPath;
            }
            catch (Exception ex)
            {
                await dialogCoordinator.ShowMessageAsync(_context, "Fehler","Das Herunterladen des Installers ist fehlgeschlagen.");
                LoggerService.Error(ex, "Fehler beim Herunterladen des Installers.");
                return null;
            }
        }

        /// <summary>
        /// Starts the installer.
        /// </summary>
        private async Task<bool> RunInstaller(string installerPath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true
                    
            });
                return true;
            }
            catch (Exception ex)
            {
                await dialogCoordinator.ShowMessageAsync(_context, "Fehler", "Das Update konnte nicht gestartet werden.");
                LoggerService.Error(ex, "Fehler beim Starten des Installers.");
                return false;
            }
        }
        private bool ProgramIsInstalled(ProgramsInfo program)
        {
            try
            {
                string basePath = ConfigurationProvider.Settings.ProgramBasePath;

                string programPath = Path.Combine(basePath, program.FolderName);
                return Directory.Exists(programPath);//letezik_e a program könyvtára a helyi RstabExternal gyokereben
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "Hiba történt a ProgramIsInstalled ellenőrzés során.");
                return false;
            }
        }



    }
}
