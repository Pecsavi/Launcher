using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
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
                ConfigurationProvider.LoadVersionsFromPath();

                ProgramUpdater.ServerVersions = ConfigurationProvider.Versions;

                string basePath = ConfigurationProvider.Settings.ProgramBasePath;
                var localFolders = Directory.GetDirectories(basePath)
                                            .Select(Path.GetFileName)
                                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                using var client = new HttpClient();

                foreach (var kvp in ProgramUpdater.ServerVersions)
                {
                    string programName = kvp.Key;
                    var info = kvp.Value;

                    if (programName == "Launcher")
                        continue;

                    var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, info.Installer));
                    if (!response.IsSuccessStatusCode)
                    {
                        LoggerService.Warn($"Installer für {programName} nicht gefunden: {info.Installer}");
                        continue;
                    }

                    ItemsToInstall.Add(new UpdateItem
                    {
                        ProgramName = programName,
                        UpdateType = localFolders.Contains(info.FolderName) ? "Neue Version" : "Neues Programm",
                        IsSelected = true
                    });
                }

                UpdateItemsList.ItemsSource = ItemsToInstall;
            }
            catch (Exception ex)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(this, "Fehler", $"Fehler beim Laden der Updates:\n{ex.Message}");
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ItemsToInstall.Where(i => i.IsSelected))
            {
                if (ProgramUpdater.ServerVersions.TryGetValue(item.ProgramName, out var info))
                {
                    await ProgramUpdater.InstallProgramAsync(info);
                }
            }
        }

        public class UpdateItem
        {
            public string ProgramName { get; set; }
            public string UpdateType { get; set; } // "Neue Version" or "Neues Programm"
            public bool IsSelected { get; set; }
        }
    }
}
