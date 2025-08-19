using Launcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;

public static class SetupFileChecker
{
    public static async Task<List<(string ProgramName, string Status)>> ValidateAndDownloadInstallers(
        List<(string ProgramName, string Status)> updateList,
        Dictionary<string, ProgramsInfo> remotePrograms,
        string updatePath)
    {
        var validUpdates = new List<(string ProgramName, string Status)>();

        foreach (var (programName, status) in updateList)
        {
            if (!remotePrograms.TryGetValue(programName, out var info))
            {
                LoggerService.Error($"Missing ProgramsInfo for {programName}");
                continue;
            }

            string installerFileName = info.Installer;                             
            string installerUrl = $"{updatePath.TrimEnd('/')}/{installerFileName}";

            try
            {
                // Check: SetupFile exists
                if (!await RemoteFileExists(installerUrl))
                {
                    LoggerService.Error($"Installer not found: {installerUrl}");
                    continue;
                }

                // Download

                string localPath = Path.Combine(Path.GetTempPath(), installerFileName);
                using var client = new HttpClient();
                var fileBytes = await client.GetByteArrayAsync(installerUrl);
                await File.WriteAllBytesAsync(localPath, fileBytes);

                // Check : Digital Signature
                if (!DigitalSignatureVerifier.IsValid(localPath))

                {
                    LoggerService.Error($"Invalid digital signature: {installerFileName}");
                    continue;
                }

                validUpdates.Add((programName, status));
            }
            catch (Exception ex)
            {
                LoggerService.Error($"Error processing {programName}: {ex.Message}");
            }
        }

        return validUpdates;
    }


    private static async Task<bool> RemoteFileExists(string url)
    {
        try
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Head, url);

            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;

        }
        catch
        {
            return false;
        }
    }
}
