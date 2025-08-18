using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
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
                var validUpdates = await UpdateManager.GetValidUpdatesAsync();
                if (validUpdates.Count == 0)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(this, "Info", "Minden program naprakész.");
                    UpdateItemsList.ItemsSource = null;
                    return;
                }

                ItemsToInstall = validUpdates.Select(u => new UpdateItem
                {
                    ProgramName = u.ProgramName,
                    UpdateType = u.UpdateType,
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
