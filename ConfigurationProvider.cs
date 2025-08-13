using Launcher;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;


namespace Launcher
{
    public static class ConfigurationProvider
    {
        public static ConfigInfo Settings { get; private set; }
        public static Dictionary<string, ProgramsInfo> Versions { get; private set; }

        static ConfigurationProvider()
        {
            LoadSettings();
        }

        private static void LoadSettings()
        {
            var json = File.ReadAllText("settings.json");
            Settings = JsonConvert.DeserializeObject<ConfigInfo>(json);
        }

        private static void LoadVersions()
        {
            var json = File.ReadAllText("programsInfo.json");
            Versions = JsonConvert.DeserializeObject<Dictionary<string, ProgramsInfo>>(json);
        }

        public static string GetSetting(string key)
        {
            var prop = typeof(ConfigInfo).GetProperty(key);
            return prop?.GetValue(Settings)?.ToString();
        }

        public static ProgramsInfo GetProgramInfo(string programName)
        {
            return Versions.TryGetValue(programName, out var info) ? info : null;
        }
        public static void LoadVersionsFromPath()
        {
            string versionPath = Settings?.VersionPath;
            if (string.IsNullOrWhiteSpace(versionPath) || !File.Exists(versionPath))
            {
                Versions = new Dictionary<string, ProgramsInfo>();
                return;
            }

            var json = File.ReadAllText(versionPath);
            Versions = JsonConvert.DeserializeObject<Dictionary<string, ProgramsInfo>>(json);
        }

    }
}
