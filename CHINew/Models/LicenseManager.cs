using CHI.Infrastructure;
using CHI.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace CHI
{
    /// <summary>
    /// Представляет менеджер лицензий, через который загружается и проверяется пользовательская лицензия
    /// </summary>
    public class LicenseManager : ILicenseManager
    {
        protected readonly RSACryptoServiceProvider cryptoProvider;
        protected static readonly string publicKeyName = "licensing.pkey";
        protected static readonly string defaultFolder = "Licensing";

        /// <summary>
        /// Стандартная директория для подсистемы лицензирования
        /// </summary>
        public string DefaultDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), defaultFolder);
        /// <summary>
        /// Расширение файла лицензии
        /// </summary>
        public string LicenseExtension { get; } = ".lic";
        /// <summary>
        /// Текущая пользовательская лицензия
        /// </summary>
        public License ActiveLicense { get; set; }
        
        /// <summary>
        /// Конструктор по-умочанию, вызывает инициализацию класса.
        /// </summary>
        public LicenseManager()
        {
            DefaultDirectory = Path.Combine(Directory.GetCurrentDirectory(), defaultFolder);

            if (!Directory.Exists(DefaultDirectory))
                Directory.CreateDirectory(DefaultDirectory);

            cryptoProvider = new RSACryptoServiceProvider();

            Initialize();
        }

        /// <summary>
        /// Инициализирует класс: Загружает ключ проверки подписи, загружает пользовательскую лицензию и проверяет ее валидность.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает когда не найден ключ проверки подписи или найдено более одной лицензии</exception>
        public virtual void Initialize()
        {
            var publicKeyBytes = ReadResource(publicKeyName);

            if (publicKeyBytes == null)
                throw new InvalidOperationException("Ошибка инициализации менеджера лицензий: не найден ключ проверки подписи.");

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
        /// <exception cref="InvalidOperationException">Возникает когда подпись не соответствует файлу лицензии</exception>
        public License LoadLicense(string licensePath)
        {
            SignedLicense signedLicense = null;

            using (var stream = new FileStream(licensePath, FileMode.Open, FileAccess.Read))
            {
                var formatter = new XmlSerializer(typeof(SignedLicense));
                signedLicense = (SignedLicense)formatter.Deserialize(stream);

                var mstream = new MemoryStream();
                formatter = new XmlSerializer(signedLicense.License.GetType());
                formatter.Serialize(mstream, signedLicense.License);
                var licenseBytes = mstream.ToArray();

                if (!cryptoProvider.VerifyData(licenseBytes, new SHA512CryptoServiceProvider(), signedLicense.Sign))
                    throw new InvalidOperationException("Ошибка проверки лицензии: подпись не соответствует лицензии.");
            }

            return signedLicense.License;
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
            sb.AppendLine("Действующие разрешения:");            
            sb.Append("Загрузка профилактических осмотров - ");

            if (ActiveLicense.ExaminationsUnlimited)
                sb.AppendLine($"Без ограничений");
            else if (!string.IsNullOrEmpty(ActiveLicense.ExaminationsFomsCodeMO))
                sb.AppendLine($"ЛПУ с кодом ФОМС {ActiveLicense.ExaminationsFomsCodeMO}");
            else if (ActiveLicense.ExaminationsMaxDate != null)
                sb.AppendLine($"Дата осмотров до {ActiveLicense.ExaminationsMaxDate.Value.ToShortDateString()}");
            else
                sb.AppendLine($"Недоступно");

            sb.AppendLine("Прочие возможности - Без ограничений");

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
