
using System;
using System.Security.Cryptography.X509Certificates;

namespace Launcher
{
    public static class DigitalSignatureVerifier
    {
        public static bool IsValid(string exePath)
        {
            try
            {
                var cert = CertificateLoader.LoadCertificateFromSettings();

                return cert.Thumbprint.Equals(
                    "63F5F19AB3C31CEAF0E5B1284B5C4211F5F0DC1B",
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                LoggerService.Error($"Zertifikat konnte nicht geladen werden: {ex.Message}");
                return false;
            }
        }
    }
}
