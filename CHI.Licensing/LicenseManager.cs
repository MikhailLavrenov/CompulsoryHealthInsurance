using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace CHI.Licensing
{
    public class LicenseManager
    {
        private static readonly int KeySize = 2048;
        //private static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        private readonly RSACryptoServiceProvider cryptoProvider;
        private readonly bool secretKeyLoaded = false;

        internal static string secretKeyPath { get; } = $"{KeysDirectory}licensing.skey";
        internal static string publicKeyPath { get; } = $"{KeysDirectory}licensing.pkey";
        public static string KeysDirectory { get; } = $@"{Directory.GetCurrentDirectory()}\Licensing\";

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
                throw new InvalidOperationException("Ошибка инициализации менеджера лицензий: не найден ключ шифрования.");

            cryptoProvider.FromXmlString(key);
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

        public void SaveNewLicense(License license)
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

        public List<License> LoadLicenses()
        {
            var filePaths = Directory.GetFiles(KeysDirectory, ".lic").ToList();
            var licenses = new List<License>();

            foreach (var filePath in filePaths)
            {
                using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                using (var mStream = new MemoryStream())
                {
                    fStream.CopyTo(mStream);

                    var bytes = cryptoProvider.Decrypt(mStream.ToArray(), true);

                    var stream = new MemoryStream(bytes);

                    var formatter = new XmlSerializer(typeof(License));

                    licenses.Add((License)formatter.Deserialize(stream));
                }
            }

            return licenses;
        }

        public License LoadCombinedLicense()
        {
            var claims = new List<Claim>();

            LoadLicenses().ForEach(x=>claims.AddRange(x.Claims));

            return new License { Claims = claims };
        }
    }
}
