
using Newtonsoft.Json;

namespace Launcher
{
    public static class ProgramsInfoReader
    {
        public static Dictionary<string, ProgramsInfo> FetchProgramsInfo(string versionUrl)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    string json = client.DownloadString(versionUrl);
                    return JsonConvert.DeserializeObject<Dictionary<string, ProgramsInfo>>(json);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error($"Failed to fetch programsInfo.json from URL: {ex.Message}");
                return new Dictionary<string, ProgramsInfo>();
            }
        }
    }
}
