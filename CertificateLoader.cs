using System.Security.Cryptography.X509Certificates;
using System.IO;


public static class CertificateLoader
{
    public static X509Certificate2 LoadEmbeddedCertificate()
    {
        string certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "pcs.cer");
        byte[] certBytes = File.ReadAllBytes(certPath);
        return new X509Certificate2(certBytes);
    }
}
