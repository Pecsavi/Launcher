using Launcher;
using System.Diagnostics;
using System.IO;

public static class VersionComparer
{
    public static Dictionary<string, string> GetInstalledVersions(string basePath)
    {
        var installedVersions = new Dictionary<string, string>();

        foreach (var exePath in Directory.GetFiles(basePath, "*.exe", SearchOption.AllDirectories))
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                installedVersions[Path.GetFileName(exePath)] = versionInfo.FileVersion;
            }
            catch (Exception ex)
            {
                LoggerService.Error($"Failed to read version of {exePath}: {ex.Message}");
            }
        }

        return installedVersions;
    }

    public static List<(string ProgramName, string Status)> CompareVersions(
        Dictionary<string, ProgramsInfo> remotePrograms,
        Dictionary<string, string> localVersions)
    {
        var updateList = new List<(string, string)>();

        foreach (var kvp in remotePrograms)
        {
            string name = kvp.Key;
            var info = kvp.Value;
            string exeName = info.Executable;
            string remoteVersion = info.Version;

            if (!localVersions.TryGetValue(exeName, out var localVersion))
            {
                updateList.Add((name, "new program"));
            }
            else if (localVersion != remoteVersion)
            {
                updateList.Add((name, $"new version:{remoteVersion}"));
            }
        }

        return updateList;
    }
}
