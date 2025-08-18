namespace Launcher
{
    public class ProgramsInfo
    {
        public string Version { get; set; }
        public string Installer { get; set; }
        public string Executable { get; set; }
        public string FolderName { get; set; }

    }
    public class SettingsInfo
    {
        public string UpdatePath { get; set; }
        public string VersionPath { get; set; }
        public string ProgramBasePath { get; set; }
        public string CertificateUrl { get; set; }
    }

}
