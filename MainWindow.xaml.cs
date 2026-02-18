
// ============================================================================
// This project uses NLog (BSD-2-Clause License).
// Copyright (c) 2004-2023 Jaroslaw Kowalski et al.
// See: https://nlog-project.org/

// This project uses MahApps.Metro (MIT License).
// Copyright (c) .NET Foundation and Contributors.
// See: https://github.com/MahApps/MahApps.Metro


// This project uses Newtonsoft.Json (MIT License).
// Copyright (c) James Newton-King.
// See: https://www.newtonsoft.com/json
// ============================================================================

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Launcher.UpdateWindow;



namespace Launcher
{




    public partial class MainWindow : MetroWindow
    {

        public readonly IDialogCoordinator _dialogCoordinator;
        public static MainWindow Instance { get; private set; }
        string basePath;
        private DroppedFileManager droppedFileManager = new DroppedFileManager();
        public static List<(string ProgramName, string UpdateType)> ValidUpdates { get; private set; } = new();


        public MainWindow()
        {
            InitializeComponent();
            SettingsReader.LoadSettingsInfo();
            Instance = this;
           
            DialogParticipation.SetRegister(this, this);
            _dialogCoordinator = DialogCoordinator.Instance;
            LoggerService.Info("Application started");
            LoggerService.UserActivity("Sign for Server");

            Loaded += async (s, e) =>
            {
                basePath = SettingsReader.Settings.ProgramBasePath;
                if (string.IsNullOrWhiteSpace(basePath))
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Fehler", "ProgramBasePath fehlt in settings.json");
                    return;
                }
                await GenerateProgramList();
                var updates = await UpdateManager.GetValidUpdatesAsync();
                MainWindow.ValidUpdates = updates;
                OnUpdatesAvailableChanged(updates.Count > 0);

            };
            LoadDroppedFiles();
        }
        private void UpdateNotificationButton_Click(object sender, RoutedEventArgs e)
        {
            var updateWindow = new UpdateWindow();
            updateWindow.Owner = this;
            updateWindow.ShowDialog();
        }
       
        private void OnUpdatesAvailableChanged(bool hasUpdates)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateNotificationButton.Visibility = hasUpdates ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        public async Task GenerateProgramList()
        {


            if (!Directory.Exists(basePath))
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Fehler", "Der Ordner 'RstabExternal' wurde nicht gefunden.");
                LoggerService.Warn("Der Ordner 'RstabExternal' wurde nicht gefunden.");
                return;
            }

            foreach (var dir in Directory.GetDirectories(basePath))
            {
                string shortName = Path.GetFileName(dir);
                string longDesc = shortName;

                var exeFiles = Directory.GetFiles(dir, "*.exe")
                                        .Where(f => !Path.GetFileName(f).StartsWith("unins", StringComparison.OrdinalIgnoreCase))
                                        .ToList();

                exeFiles = exeFiles
                    .Where(f => !Path.GetFileName(f).Contains("Launcher.exe", StringComparison.OrdinalIgnoreCase))
                    .ToList();


                if (exeFiles.Count == 0)
                    continue;

                string exePath = exeFiles[0];

                string descFile = Path.Combine(dir, "description.txt");
                if (File.Exists(descFile))
                {
                    longDesc = File.ReadAllText(descFile).Trim();
                }

                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

                var exeButton = new Button
                {
                    Content = shortName,
                    Width = 200,
                    Height = 30,
                    Margin = new Thickness(0, 0, 10, 0)

                };
                exeButton.Click += async (s, e) =>
                {

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = Path.GetDirectoryName(exePath),
                        UseShellExecute = true
                    };

                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "Fehler", "Start fehlgeschlagen: " + ex.Message);
                        LoggerService.Warn("102 exeButton_Click start fehlgeschlagen");
                    }

                };

                exeButton.MouseRightButtonUp += (s, e) =>
                {

                    if (!string.IsNullOrWhiteSpace(longDesc))
                    {
                        FlyoutTextBlock.Text = longDesc;
                        InfoFlyout.IsOpen = true;
                    }

                };

                row.Children.Add(exeButton);

                ProgramListPanel.Children.Add(row);
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)

        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                  
                    droppedFileManager.AddFile(file);
                    LoadDroppedFiles();

                    var container = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

                    var fileButton = new Button
                    {
                        Content = Path.GetFileName(file),
                        Width = 200,
                        Height = 30,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    fileButton.Click += async (s, args) =>
                    {
                        try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); }
                        catch (Exception ex)
                        {
                            await _dialogCoordinator.ShowMessageAsync(this, "Fehler", "Konnte nicht geöffnet werden: " + ex.Message);
                            LoggerService.Warn("162. File konnte nicht geöffnet werden");
                        }
                    };

                    var contextMenu = new ContextMenu();

                    var openItem = new MenuItem { Header = "Öffnen" };
                    openItem.Click += async (s, args) =>
                    {
                        try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); }
                        catch (Exception ex)
                        {
                            await _dialogCoordinator.ShowMessageAsync(this, "Fehler", "Konnte nicht geöffnet werden: " + ex.Message);
                            LoggerService.Warn("172. File konnte nicht geöffnet werden");
                        }
                    };

                    var deleteItem = new MenuItem { Header = "Löschen" };
                    deleteItem.Click += (s, args) =>
                    {
                        FileListPanel.Children.Remove(container);
                        droppedFileManager.RemoveFile(file);
                    };
                    var renameItem = new MenuItem { Header = "Umbenennen" };
                    renameItem.Click += async (s, args) =>
                    {
                        var input = await _dialogCoordinator.ShowInputAsync(this, "Button umbenennen", "Neuer Anzeigename:");
                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            try
                            {
                                fileButton.Content = input;

                                // droppedfiles.txt frissítése: új név mentése
                                droppedFileManager.RemoveFile(file);
                                droppedFileManager.AddFile(file); // ugyanaz a fájl, de a gomb felirata már más

                                // opcionálisan: mentés külön névvel, ha a droppedfiles.txt formátuma bővíthető
                            }
                            catch (Exception ex)
                            {
                                await _dialogCoordinator.ShowMessageAsync(this, "Fehler", $"Umbenennen fehlgeschlagen: {ex.Message}");
                                LoggerService.Warn($"Button konnte nicht umbenannt werden: {ex.Message}");
                            }
                        }
                    };

                    contextMenu.Items.Add(openItem);
                    contextMenu.Items.Add(deleteItem);
                    fileButton.ContextMenu = contextMenu;

                    container.Children.Add(fileButton);
                    
                }

               

            }
        }
        private void LoadDroppedFiles()
        {
            Dispatcher.Invoke(() =>
            {

                FileListPanel.Children.Clear();

            });

            var lines = droppedFileManager.LoadFiles();
            var validFiles = lines.Where(f => File.Exists(f.FilePath)).ToList();

            if (validFiles.Count != lines.Count)
                droppedFileManager.OverwriteFiles(validFiles);

            foreach (var file in validFiles)
            {
                if (!File.Exists(file.FilePath)) continue;

                var container = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

                var fileButton = new Button
                {
                    Content = Path.GetFileName(file.FilePath),
                    Width = 200,
                    Height = 30,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                fileButton.Click += async (s, e) =>
                {
                    try { Process.Start(new ProcessStartInfo(file.FilePath) { UseShellExecute = true }); }
                    catch (Exception ex)
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "Fehler", "Konnte nicht geöffnet werden: " + ex.Message);
                        LoggerService.Warn("230. File konnte nicht geöffnet werden");
                    }
                };

                var contextMenu = new ContextMenu();

                var openItem = new MenuItem { Header = "Öffnen" };
                openItem.Click += async (s, args) =>
                {
                    try { Process.Start(new ProcessStartInfo(file.FilePath) { UseShellExecute = true }); }
                    catch (Exception ex)
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "Fehler", "Konnte nicht geöffnet werden: " + ex.Message);
                        LoggerService.Warn("240. File konnte nicht geöffnet werden");
                    }
                };

                var deleteItem = new MenuItem { Header = "Löschen" };
                deleteItem.Click += (s, args) =>
                {
                    FileListPanel.Children.Remove(container);
                    droppedFileManager.RemoveFile(file.FilePath);
                };

                contextMenu.Items.Add(openItem);
                contextMenu.Items.Add(deleteItem);
                fileButton.ContextMenu = contextMenu;

                container.Children.Add(fileButton);
                FileListPanel.Children.Add(container);
            }
        }
    }
}
