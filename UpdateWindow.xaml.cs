using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : MetroWindow
    {
        public ObservableCollection<UpdateItem> ItemsToInstall { get; set; } = new();
        public event Action<bool> UpdatesAvailableChanged;

        public UpdateWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadUpdatesAsync();
        }

        private async void LoadUpdatesAsync()
        {
            try
            {
                var validUpdates = await UpdateManager.GetValidUpdatesAsync();
                bool hasUpdates = validUpdates.Count > 0;

                UpdatesAvailableChanged?.Invoke(hasUpdates);

                if (!hasUpdates)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(this, "Info", "Alle Programme sind auf dem neuesten Stand.");
                    UpdateItemsList.ItemsSource = null;
                    return;
                }

                ItemsToInstall.Clear();
                foreach (var u in validUpdates)
                {
                    ItemsToInstall.Add(new UpdateItem
                    {
                        ProgramName = u.ProgramName,
                        UpdateType = u.UpdateType,
                        IsSelected = true
                    });
                }


                UpdateItemsList.ItemsSource = ItemsToInstall;

                
            }
            catch (Exception ex)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(this, "Fehler", $"Laden der Update-Liste fehlgeschlagen.:\n{ex.Message}");
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
