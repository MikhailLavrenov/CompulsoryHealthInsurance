namespace CHI.Application
{
    /// <summary>
    /// Представляет менеджер лицензий, через который загружается и проверяется пользовательская лицензия
    /// </summary>
    public interface ILicenseManager
    {
        /// <summary>
        /// Текущая пользовательская лицензия
        /// </summary>
        License ActiveLicense { get; set; }

        /// <summary>
        /// Возвращает описание текущей лицензии в виде строк (включая предоставленные права)
        /// </summary>
        /// <returns>описание лицензии</returns>
        string GetActiveLicenseInfo();
    }
}
