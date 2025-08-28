using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Launcher
{
    public static class DigitalSignatureVerifier
    {
        public static bool IsValid(string exePath)
        {
            try
            {
                if (!File.Exists(exePath))
                {
                    LoggerService.Warn($"Datei nicht gefunden: {exePath}");
                    return false;
                }

                var expectedThumbprint = SettingsReader.Settings.ExpectedThumbprint;
                if (string.IsNullOrWhiteSpace(expectedThumbprint))
                {
                    LoggerService.Warn("ExpectedThumbprint fehlt in settings.json");
                    return false;
                }

                X509Certificate cert;
                try
                {
                    cert = X509Certificate.CreateFromSignedFile(exePath);
                }
                catch (Exception ex)
                {
                    LoggerService.Warn($"Keine digitale Signatur gefunden in Datei: {exePath} – {ex.Message}");
                    return false;
                }

                var actualThumbprint = cert.GetCertHashString();
                if (!actualThumbprint.Equals(expectedThumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    LoggerService.Warn($"Thumbprint stimmt nicht überein für Datei: {exePath}. Erwartet: {expectedThumbprint}, gefunden: {actualThumbprint}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggerService.Error($"Digitale Signaturprüfung fehlgeschlagen für Datei: {exePath} – {ex.Message}");
                return false;
            }
        }

    }
}
