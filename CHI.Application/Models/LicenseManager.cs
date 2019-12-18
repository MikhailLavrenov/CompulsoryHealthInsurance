using CHI.Application.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace CHI.Application
{
    /// <summary>
    /// Представляет менеджер лицензий, через который загружается и проверяется пользовательская лицензия
    /// </summary>
    public class LicenseManager : ILicenseManager
    {
        protected readonly RSACryptoServiceProvider cryptoProvider;
        protected static readonly string publicKeyName = "licensing.pkey";

        /// <summary>
        /// Стандартная директория для подсистемы лицензирования
        /// </summary>
        public static string DefaultDirectory { get; } = $@"{Directory.GetCurrentDirectory()}\Licensing\";
        /// <summary>
        /// Расширение файла проверки лицензии
        /// </summary>
        public static string SignExtension { get; } = ".sig";
        /// <summary>
        /// Расширение файла лицензии
        /// </summary>
        public static string LicenseExtension { get; } = ".lic";
        /// <summary>
        /// Текущая пользовательская лицензия
        /// </summary>
        public License ActiveLicense { get; set; }
        
        /// <summary>
        /// Конструктор по-умочанию, вызывает инициализацию класса.
        /// </summary>
        public LicenseManager()
        {
            cryptoProvider = new RSACryptoServiceProvider();

            Initialize();
        }

        /// <summary>
        /// Инициализирует класс: Загружает ключ проверки подписи, загружает пользовательскую лицензию и проверяет ее валидность.
        /// </summary>
        public virtual void Initialize()
        {
            var publicKeyBytes = ReadResource(publicKeyName);

            if (publicKeyBytes == null)
                throw new InvalidOperationException("Ошибка инициализации менеджера лицензий: не найден криптографический ключ.");

            cryptoProvider.ImportCspBlob(publicKeyBytes);

            var licensePaths = Directory.GetFiles(DefaultDirectory, $"*{LicenseExtension}").ToList();

            if (licensePaths.Count > 1)
                throw new InvalidOperationException("Ошибка загрузки лицензии: лицензий не может быть больше одной.");

            if (licensePaths.Count == 1)
                ActiveLicense = LoadLicense(licensePaths.First());
        }
        /// <summary>
        /// Загружает лицензию по заданному пути.
        /// </summary>
        /// <param name="licensePath">Путь для загружки лицензии</param>
        /// <returns>Загруженная лицензия</returns>
        public License LoadLicense(string licensePath)
        {
            License license = null;

            var signPath = Path.ChangeExtension(licensePath, SignExtension);

            using (var licenseStream = new FileStream(licensePath, FileMode.Open, FileAccess.Read))
            using (var signStream = new FileStream(signPath, FileMode.Open, FileAccess.Read))
            {
                var licenseBytes = licenseStream.GetBytes();
                var signBytes = signStream.GetBytes();

                if (cryptoProvider.VerifyData(licenseBytes, new SHA512CryptoServiceProvider(), signBytes))
                {
                    var formatter = new XmlSerializer(typeof(License));

                    licenseStream.Position = 0;
                    license = (License)formatter.Deserialize(licenseStream);
                }
                else
                    throw new InvalidOperationException("Ошибка проверки лицензии: подпись не соответствует лицензии.");
            }

            return license;
        }
        /// <summary>
        /// Возвращает описание текущей лицензии в виде строк (включая предоставленные права)
        /// </summary>
        /// <returns>описание лицензии</returns>
        public string GetActiveLicenseInfo()
        {
            if (ActiveLicense == null)
                return "Отсутствует";

            var sb = new StringBuilder();

            sb.AppendLine($"Выдана: {ActiveLicense.Owner}");
            sb.AppendLine($"Активные разрешения:");
            sb.Append($@"Загрузка профилактических осмотров - ");

            if (ActiveLicense.ExaminationsUnlimited)
                sb.Append($"Без ограничений");
            else if (!string.IsNullOrEmpty(ActiveLicense.ExaminationsFomsCodeMO))
                sb.Append($"ЛПУ с кодом ФОМС {ActiveLicense.ExaminationsFomsCodeMO}");
            else if (ActiveLicense.ExaminationsMaxDate != null)
                sb.Append($"Дата осмотров до {ActiveLicense.ExaminationsMaxDate.Value.ToShortDateString()}");
            else
                sb.Append($"Недоступно");

            return sb.ToString();
        }
        /// <summary>
        /// Возвращает массив байт заданного ресурса текущей сборки.
        /// </summary>
        /// <param name="name">Название ресурса</param>
        /// <returns>Массив байт ресурса сборки</returns>
        protected static byte[] ReadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(name));

            byte[] result = null;


            if (!string.IsNullOrEmpty(resourcePath))
                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    result = stream.GetBytes();
                }

            return result;
        }
    }
}
