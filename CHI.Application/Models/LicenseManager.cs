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
    public class LicenseManager : ILicenseManager
    {
        protected readonly RSACryptoServiceProvider cryptoProvider;
        protected static readonly string publicKeyName = "licensing.pkey";

        public static string DefaultDirectory { get; } = $@"{Directory.GetCurrentDirectory()}\Licensing\";
        public static string SignExtension { get; } = ".sig";
        public static string LicenseExtension { get; } = ".lic";
        public License ActiveLicense { get; set; }
        
        public LicenseManager()
        {
            cryptoProvider = new RSACryptoServiceProvider();

            Initialize();
        }

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
