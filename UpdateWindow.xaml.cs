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
            LoadUpdates(MainWindow.ValidUpdates);

            this.Closed += UpdateWindow_Closed;

        }

        private void LoadUpdates(List<(string ProgramName, string UpdateType)> validUpdates)
        {
            bool hasUpdates = validUpdates.Count > 0;
            if (!hasUpdates)
            {
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

        private async void InstallButton_Click(object? sender, RoutedEventArgs e)
        {
            var selectedUpdates = ItemsToInstall.Where(i => i.IsSelected).ToList();
            await ProgramInstaller.InstallAsync(selectedUpdates.Select(i => (i.ProgramName, i.UpdateType)).ToList(), this);

            foreach (var item in selectedUpdates)
            {
                var toRemove = ItemsToInstall.FirstOrDefault(i => i.ProgramName == item.ProgramName);
                if (toRemove != null)
                    ItemsToInstall.Remove(toRemove);
            }

            if (ItemsToInstall.Count == 0)
            {
                if (MainWindow.Instance?.UpdateNotificationButton != null)
                    MainWindow.Instance.UpdateNotificationButton.Visibility = System.Windows.Visibility.Collapsed;
                this.Close();
            }

        }

        private async void UpdateWindow_Closed(object sender, EventArgs e)
        {

            await Dispatcher.InvokeAsync(async () =>
            {
                MainWindow.Instance?.ProgramListPanel.Children.Clear();
                await MainWindow.Instance?.GenerateProgramList();
            });

        }


        public class UpdateItem
        {
            public string ProgramName { get; set; }
            public string UpdateType { get; set; } // "Neue Version" or "Neues Programm"
            public bool IsSelected { get; set; }
        }
    }
}
