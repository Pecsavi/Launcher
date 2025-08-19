using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Launcher
{
    public static class UpdateManager
    {
        public static async Task<List<(string ProgramName, string UpdateType)>> GetValidUpdatesAsync()
        {
            LoggerService.Info("Update process started.");

            // 1. Load Settings
            SettingsReader.LoadSettingsInfo();
            if (SettingsReader.Settings == null)
            {
                LoggerService.Error("Settings not loaded. Update aborted.");
                return new();
            }

            // 2. Check internet connection and critical paths
            if (!SettingsReader.IsInternetAvailable())
            {
                LoggerService.Error("No internet connection. Update aborted.");
                return new();
            }

            var result = await SettingsReader.ValidateCriticalPathsAsync();
            if (!result.IsValid)
            {
                LoggerService.Error($"Critical path validation failed:\n{result.ErrorMessage}");
                return new();
            }

            // 3. Read ProgramsInfo.json
            var remotePrograms = ProgramsInfoReader.FetchProgramsInfo(SettingsReader.Settings.VersionPath);
            if (remotePrograms.Count == 0)
            {
                LoggerService.Error("No remote program info found. Update aborted.");
                return new();
            }

            // 4. Read local versions
            var localVersions = VersionComparer.GetInstalledVersions(SettingsReader.Settings.ProgramBasePath);

            // 5. Compare versions
            var updateList = VersionComparer.CompareVersions(remotePrograms, localVersions);
            if (updateList.Count == 0)
            {
                LoggerService.Info("All programs are up to date.");
                return new();
            }

            // 6. Check and download installers
            var validUpdates = await SetupFileChecker.ValidateAndDownloadInstallers(
                updateList.Select(u => (u.ProgramName, u.Status)).ToList(),
                remotePrograms,
                SettingsReader.Settings.UpdatePath);

            if (validUpdates.Count == 0)
            {
                LoggerService.Error("No valid installers found. Update aborted.");
                return new();
            }

            return validUpdates;
        }
    }
}
