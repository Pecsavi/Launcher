using Launcher;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;

public static class DigitalSignatureVerifier
{

    public static bool IsValid(string exePath)
    {
        try
        {
            string certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "pcs.cer");
            byte[] certBytes = File.ReadAllBytes(certPath);
            var cert = new X509Certificate2(certBytes);

            return cert.Subject.Contains("Csaba Viktor Perényi");
        }
        catch (Exception ex)
        {
            LoggerService.Error($"Tanúsítvány betöltése sikertelen: {ex.Message}");
            return false;
        }
    }

}