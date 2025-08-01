
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
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Verrollungsnachweis;


namespace Launcher
{
    

    public partial class MainWindow : MetroWindow
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public MainWindow()
        {
            InitializeComponent();

            LoggerService.Info("Application started");
            LoggerService.UserActivity("Application started");
            GenerateProgramList();
            LoadDroppedFiles();
        }

        private void GenerateProgramList()
        {
            string basePath = @"C:\Program Files (x86)\RstabExternal";
            if (!Directory.Exists(basePath))
            {
                MessageBox.Show("Der Ordner 'RstabExternal' wurde nicht gefunden.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
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
                exeButton.Click += (s, e) =>
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
                        MessageBox.Show("Start fehlgeschlagen: " + ex.Message);
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
                var existing = File.Exists("droppedfiles.txt") ? File.ReadAllLines("droppedfiles.txt").ToList() : new List<string>();

               

                foreach (var file in files)
                {

                    if (!existing.Contains(file))
                        existing.Add(file);

                    var container = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

                    var fileButton = new Button
                    {
                        Content = Path.GetFileName(file),
                        Width = 200,
                        Height = 30, 
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    fileButton.Click += (s, args) =>
                    {
                        try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); }
                        catch (Exception ex) { MessageBox.Show("Konnte nicht geöffnet werden: " + ex.Message); LoggerService.Warn("162. File konnte nicht geöffnet werden"); }
                    };

                    // Kontextusmenü létrehozása
                    var contextMenu = new ContextMenu();

                    var openItem = new MenuItem { Header = "Öffnen" };
                    openItem.Click += (s, args) =>
                    {
                        try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); }
                        catch (Exception ex) { MessageBox.Show("Konnte nicht geöffnet werden: " + ex.Message); LoggerService.Warn("172. File konnte nicht geöffnet werden"); }
                    };

                    var deleteItem = new MenuItem { Header = "Löschen" };
                    deleteItem.Click += (s, args) =>
                    {
                        FileListPanel.Children.Remove(container);
                        existing.Remove(file);
                        File.WriteAllLines("droppedfiles.txt", existing);
                    };

                    contextMenu.Items.Add(openItem);
                    contextMenu.Items.Add(deleteItem);
                    fileButton.ContextMenu = contextMenu;


                    container.Children.Add(fileButton);
     

                    FileListPanel.Children.Add(container);

//ProgramListPanel.Children.Add(fileButton);
                }

                File.WriteAllLines("droppedfiles.txt", existing);
            }
        }


        private void LoadDroppedFiles()
        {
            var existing = File.Exists("droppedfiles.txt")
    ? File.ReadAllLines("droppedfiles.txt").ToList()
    : new List<string>();

            string filePath = "droppedfiles.txt";
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath);
            foreach (var file in lines)
            {
                if (!File.Exists(file)) continue;

                var container = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

                var fileButton = new Button
                {
                    Content = Path.GetFileName(file),
                    Width = 200,
                    Height = 30,
                    Margin = new Thickness(0, 5, 0, 0)
                };


                fileButton.Click += (s, e) =>
                {
                    try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); }
                    catch (Exception ex) { MessageBox.Show("Konnte nicht geöffnet werden: " + ex.Message); LoggerService.Warn("230. File konnte nicht geöffnet werden"); }
                };

                // Kontextusmenü létrehozása
                var contextMenu = new ContextMenu();

                var openItem = new MenuItem { Header = "Öffnen" };
                openItem.Click += (s, args) =>
                {
                    try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); }
                    catch (Exception ex) { MessageBox.Show("Konnte nicht geöffnet werden: " + ex.Message); LoggerService.Warn("240. File konnte nicht geöffnet werden"); }
                };

                var deleteItem = new MenuItem { Header = "Löschen" };
                deleteItem.Click += (s, args) =>
                {
                    FileListPanel.Children.Remove(container);
                    existing.Remove(file);
                    File.WriteAllLines("droppedfiles.txt", existing);
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
