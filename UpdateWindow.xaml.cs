using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;

namespace Launcher
{
    public partial class UpdateWindow : MetroWindow
    {

        public List<UpdateItem> ItemsToInstall { get; set; } = new();

        public UpdateWindow()
        {
            InitializeComponent();
            LoadUpdatesAsync();

        }

        private async void LoadUpdatesAsync()
        {
            try
            {
                SettingsReader.LoadSettingsInfo();
                if (SettingsReader.Settings == null)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(this, "Hiba", "A settings.json nem tölthető be.");
                    return;
                }

                string errorMessage = string.Empty;
                if (!SettingsReader.IsInternetAvailable() ||
                    !SettingsReader.ValidateCriticalPaths(out errorMessage))
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(this, "Hiba", $"Kritikus elérési útvonal hiba:\n{errorMessage}");
                    return;
                }

                var remotePrograms = ProgramsInfoReader.FetchProgramsInfo(SettingsReader.Settings.VersionPath);
                var localVersions = VersionComparer.GetInstalledVersions(SettingsReader.Settings.ProgramBasePath);
                var updateList = VersionComparer.CompareVersions(remotePrograms, localVersions);
                // Validate installers and download them
                var validUpdates = await SetupFileChecker.ValidateAndDownloadInstallers(
                    updateList.Select(u => (u.ProgramName, u.Status)).ToList(),
                    remotePrograms,
                    SettingsReader.Settings.VersionPath // or the correct path to installers
                );

                if (validUpdates.Count == 0)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(this, "Info", "Minden program naprakész.");
                    UpdateItemsList.ItemsSource = null;
                    return;
                }
                ItemsToInstall = updateList.Select(u => new UpdateItem
                {
                    ProgramName = u.ProgramName,
                    UpdateType = u.Status,
                    IsSelected = true
                }).ToList();

                UpdateItemsList.ItemsSource = ItemsToInstall;
            }
            catch (Exception ex)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(this, "Hiba", $"Frissítési lista betöltése sikertelen:\n{ex.Message}");
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUpdates = ItemsToInstall.Where(i => i.IsSelected).ToList();
            await ProgramInstaller.InstallAsync(selectedUpdates.Select(i => (i.ProgramName, i.UpdateType)).ToList());
            
        }

        public class UpdateItem
        {
            public string ProgramName { get; set; }
            public string UpdateType { get; set; } // "Neue Version" or "Neues Programm"
            public bool IsSelected { get; set; }
        }
    }
}
