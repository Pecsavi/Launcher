using Newtonsoft.Json;
using System.IO;


namespace Launcher
{
    public static class SettingsReader
    {
        public static SettingsInfo Settings { get; private set; }
        
        public static void LoadSettingsInfo()
        {
            try
            {
                var json = File.ReadAllText("settings.json");
                Settings = JsonConvert.DeserializeObject<SettingsInfo>(json);
            }
            catch (Exception ex)
            {
                LoggerService.Error($"Failed to load settings.json: {ex.Message}");
                return;
            }
           
        }

        public static string GetSetting(string key)
        {
            var prop = typeof(SettingsInfo).GetProperty(key);
            return prop?.GetValue(Settings)?.ToString();
        }
        public static bool ValidateCriticalPaths(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (Settings == null)
            {
                errorMessage = "Settings not loaded.";
                return false;
            }

            // Check VersionPath
            if (string.IsNullOrWhiteSpace(Settings.VersionPath))
            {
                errorMessage += "Missing VersionPath.\n";
            }
            else if (Settings.VersionPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Check if URL is reachable
                try
                {
                    var request = System.Net.WebRequest.Create(Settings.VersionPath);
                    request.Method = "HEAD";
                    using (var response = request.GetResponse()) { }
                }
                catch
                {
                    errorMessage += "VersionPath URL is not reachable.\n";
                }
            }
            else if (!File.Exists(Settings.VersionPath))
            {
                errorMessage += "VersionPath file does not exist.\n";
            }

            if (string.IsNullOrWhiteSpace(Settings.ProgramBasePath) || !Directory.Exists(Settings.ProgramBasePath))
                errorMessage += "Missing or invalid ProgramBasePath.\n";

            if (string.IsNullOrWhiteSpace(Settings.CertificateUrl))
                errorMessage += "Missing CertificateUrl.\n";

            return string.IsNullOrEmpty(errorMessage);
        }
        public static bool IsInternetAvailable()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (client.OpenRead("http://www.google.com"))
                    return true;
            }
            catch
            {
                LoggerService.Error("No internet connection available.");
                return false;
            }
        }
    }
}
