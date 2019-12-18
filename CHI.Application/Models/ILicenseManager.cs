namespace CHI.Application
{
    public interface ILicenseManager
    {
        License ActiveLicense { get; set; }

        string GetActiveLicenseInfo();
    }
}
