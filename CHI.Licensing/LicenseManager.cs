using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace CHI.Licensing
{
    public class LicenseManager : ILicenseManager
    {
        private static readonly int KeySize = 2048;
        //private static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        private readonly RSACryptoServiceProvider cryptoProvider;
        private readonly bool secretKeyLoaded = false;

        public static string KeysDirectory { get; } = $@"{Directory.GetCurrentDirectory()}\Licensing\";
        internal static string secretKeyPath { get; } = $"{KeysDirectory}licensing.skey";
        internal static string publicKeyPath { get; } = $"{KeysDirectory}licensing.pkey";
        
        public License ActiveLicense { get; set; }

        public LicenseManager()
        {
            string key;

            if (File.Exists(secretKeyPath))
            {
                key = File.ReadAllText(secretKeyPath);
                secretKeyLoaded = true;
            }
            else if (File.Exists(publicKeyPath))
                key = File.ReadAllText(publicKeyPath);
            else
                throw new InvalidOperationException("Ошибка инициализации менеджера лицензий: не найден криптографический ключ.");

            cryptoProvider = new RSACryptoServiceProvider();
            cryptoProvider.FromXmlString(key);

            var licensePaths = new DirectoryInfo(KeysDirectory).GetFiles(".lic").OrderBy(x => x.CreationTime).ToList();

            if (licensePaths.Count>0)
                ActiveLicense = LoadLicense(licensePaths.First().FullName);
        }

        internal static void GenerateNewKeyPair()
        {
            new FileInfo(KeysDirectory).Directory.Create();

            using (var rsaProvider = new RSACryptoServiceProvider(KeySize))
            {
                var secretKey = rsaProvider.ToXmlString(true);
                File.WriteAllText(secretKeyPath, secretKey);

                var publicKey = rsaProvider.ToXmlString(false);
                File.WriteAllText(publicKeyPath, publicKey);
            }
        }

        public void SaveLicense(License license)
        {
            if (!secretKeyLoaded)
                throw new InvalidOperationException("Ошибка генерации лицензии: отсутствует закрытый ключ.");

            var dateTimeStr = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_FFF");
            var filePath = $"{KeysDirectory}License {dateTimeStr}.lic";


            using (var mStream = new MemoryStream())
            using (var fStream = new FileStream(filePath, FileMode.CreateNew))
            {
                var formatter = new XmlSerializer(license.GetType());

                formatter.Serialize(mStream, license);

                var encryptedBytes = cryptoProvider.Encrypt(mStream.ToArray(), true);

                fStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            }
        }

        public License LoadLicense(string path)
        {
            License license;

            using (var fStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (var mStream = new MemoryStream())
            {
                fStream.CopyTo(mStream);

                var bytes = cryptoProvider.Decrypt(mStream.ToArray(), true);

                var stream = new MemoryStream(bytes);

                var formatter = new XmlSerializer(typeof(License));

                license = (License)formatter.Deserialize(stream);
            }

            return license;
        }

        public List<Claim> GetClaims(Type type)
        {
            return ActiveLicense?.Claims?.Where(x => x.TargetType == type).ToList()??new List<Claim>();
        }
    }
}

