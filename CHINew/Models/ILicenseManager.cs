namespace CHI
{
    /// <summary>
    /// Представляет менеджер лицензий, через который загружается и проверяется пользовательская лицензия
    /// </summary>
    public interface ILicenseManager
    {
        /// <summary>
        /// Стандартная директория расположения лицензий
        /// </summary>
        string DefaultDirectory { get; }
        /// <summary>
        /// Расширение файла лицензии
        /// </summary>
        string LicenseExtension { get; }
        /// <summary>
        /// Текущая загруженная пользовательская лицензия
        /// </summary>
        License ActiveLicense { get; set; }
        /// <summary>
        /// Возвращает описание текущей лицензии в виде строк (включая предоставленные права)
        /// </summary>
        /// <returns>описание лицензии</returns>
        string GetActiveLicenseInfo();
        /// <summary>
        /// Инициализирует лицензию
        /// </summary>
        void Initialize();
    }
}
