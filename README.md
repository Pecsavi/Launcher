# WPF Launcher

This is a simple WPF-based launcher application designed to start external engineering tools located in a predefined folder. It dynamically lists available `.exe` files and allows launching them via a graphical interface. The application also supports drag-and-drop file handling and persists dropped files for later access.

## Features

- Lists `.exe` files from `C:\Program Files (x86)\RstabExternal` and displays them as buttons.
- Launches external programs with a single click.
- Shows descriptions from `description.txt` on right-click.
- Supports drag-and-drop of files and remembers them across sessions.
- Context menu for opening or removing dropped files.

## Used Libraries

- [NLog](https://nlog-project.org/) – BSD-2-Clause License
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) – MIT License
- [Newtonsoft.Json](https://www.newtonsoft.com/json) – MIT License

## Usage

1. Place your external tools in subfolders under `C:\Program Files (x86)\RstabExternal`.
2. Optionally include a `description.txt` file in each subfolder for tool descriptions.
3. Run the launcher to see and start the tools.
4. Drag files into the window to store and reopen them later.

## License

This project is released as open source and is publicly available.  
