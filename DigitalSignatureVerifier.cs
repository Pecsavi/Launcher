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

                // Az elvárt kiadó neve (Subject SimpleName)
                var expectedPublisher = SettingsReader.Settings.ExpectedPublisherCN;
                if (string.IsNullOrWhiteSpace(expectedPublisher))
                {
                    LoggerService.Warn("ExpectedPublisherCN fehlt in settings.json");
                    return false;
                }

                // 1) Beágyazott Authenticode aláírás megnyitása
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

                var signer = new X509Certificate2(cert);

                // 2) Kiadó ellenőrzése
                var simpleName = signer.GetNameInfo(X509NameType.SimpleName, false);
                if (!simpleName.Equals(expectedPublisher, StringComparison.OrdinalIgnoreCase))
                {
                    LoggerService.Warn($"Publisher stimmt nicht überein. Erwartet: {expectedPublisher}, gefunden: {simpleName}");
                    return false;
                }

                // 3) Lánc validálása trusted rootig
                using (var chain = new X509Chain())
                {
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

                    if (!chain.Build(signer))
                    {
                        LoggerService.Warn("Zertifikatskette ungültig (Chain.Build fehlgeschlagen).");
                        return false;
                    }
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