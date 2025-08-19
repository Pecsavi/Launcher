using Newtonsoft.Json;
using System.IO;
using System.Net.Http;


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
        public static async Task<(bool IsValid, string ErrorMessage)> ValidateCriticalPathsAsync()
        {
            string errorMessage = string.Empty;
            if (Settings == null)
            {
                errorMessage = "Settings not loaded.";
                return (string.IsNullOrEmpty(errorMessage), errorMessage);
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
                    using (var client = new HttpClient())
                    {
                        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, Settings.VersionPath));
                        if (!response.IsSuccessStatusCode)
                        {
                            errorMessage += "VersionPath URL is not reachable.\n";
                        }
                    }
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

            return (string.IsNullOrEmpty(errorMessage), errorMessage);
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
