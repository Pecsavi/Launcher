using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace Launcher
{
    public static class VersionService
    {
        private static readonly HttpClient client = new HttpClient();


        public static async Task<VersionInfoResult> GetVersionInfoAsync()
        {
            ConfigurationProvider.LoadVersionsFromPath();
            string versionUrl = ConfigurationProvider.Settings.VersionPath;

            string json = await FetchVersionJsonAsync(versionUrl);
            if (string.IsNullOrEmpty(json))
                return null;


            var serverVersions = ConfigurationProvider.Versions;

            return new VersionInfoResult
            {
                Programs = serverVersions.Values,
                ProgramDictionary = serverVersions
            };
        }

        private static async Task<string> FetchVersionJsonAsync(string url)
        {
            try
            {
                return await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, $"Die Datei programsInfo.json konnte von der URL {url} nicht heruntergeladen werden.");
                return null;
            }
        }
    }

    public class VersionInfoResult
    {
        public IEnumerable<ProgramsInfo> Programs { get; set; }
        public Dictionary<string, ProgramsInfo> ProgramDictionary { get; set; }
    }
}
