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

        public bool SecretKeyLoaded { get; }
        public static string DefaultDirectory { get; } = $@"{Directory.GetCurrentDirectory()}\Licensing\";
        internal static string secretKeyPath { get; } = $"{DefaultDirectory}licensing.skey";
        internal static string publicKeyPath { get; } = $"{DefaultDirectory}licensing.pkey";

        public License ActiveLicense { get; set; }

        public LicenseManager()
        {
            string key;           

            if (File.Exists(secretKeyPath))
            {
                key = File.ReadAllText(secretKeyPath);
                SecretKeyLoaded = true;
            }
            else if (File.Exists(publicKeyPath))
            {
                key = File.ReadAllText(publicKeyPath);
                SecretKeyLoaded = false;
            }
            else
                throw new InvalidOperationException("Ошибка инициализации менеджера лицензий: не найден криптографический ключ.");

            cryptoProvider = new RSACryptoServiceProvider();
            cryptoProvider.FromXmlString(key);

            var licensePaths = new DirectoryInfo(DefaultDirectory).GetFiles(".lic").OrderBy(x => x.CreationTime).ToList();

            if (licensePaths.Count > 0)
                ActiveLicense = LoadLicense(licensePaths.First().FullName);
        }

        internal static void GenerateNewKeyPair()
        {
            new FileInfo(DefaultDirectory).Directory.Create();

            using (var rsaProvider = new RSACryptoServiceProvider(KeySize))
            {
                var secretKey = rsaProvider.ToXmlString(true);
                File.WriteAllText(secretKeyPath, secretKey);

                var publicKey = rsaProvider.ToXmlString(false);
                File.WriteAllText(publicKeyPath, publicKey);
            }
        }

        public void SaveLicense(License license, string path)
        {
            if (!SecretKeyLoaded)
                throw new InvalidOperationException("Ошибка генерации лицензии: отсутствует закрытый ключ.");

            using (var mStream = new MemoryStream())
            using (var fStream = new FileStream(path, FileMode.CreateNew))
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
    }
}

