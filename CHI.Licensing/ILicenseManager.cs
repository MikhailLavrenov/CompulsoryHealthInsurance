namespace CHI.Licensing
{
    public interface ILicenseManager
    {
        License ActiveLicense { get; set; }
        bool SecretKeyLoaded { get; }

        License LoadLicense(string path);
        void SaveLicense(License license, string path);
        string GetLicenseInfo();
    }
}
