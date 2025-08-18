using Launcher;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;


public static class CertificateLoader
{
    

    public static X509Certificate2 LoadCertificateFromSettings()
    {
        string url = SettingsReader.Settings.CertificateUrl;
        using (var client = new WebClient())
        {
            byte[] certBytes = client.DownloadData(url);
            return new X509Certificate2(certBytes);
        }
    }

}
