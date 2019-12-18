using CHI.Application;
using CHI.Application.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace CHI.Licensing
{
    public class LicenseAdmin : LicenseManager
    {
        private static readonly int KeySize = 2048;
        private static readonly string secretKeyName = "licensing.skey";

        public bool SecretKeyLoaded { get; private set; }
        internal static string secretKeyPath { get; } = $"{DefaultDirectory}{secretKeyName}";
        internal static string publicKeyPath { get; } = $"{DefaultDirectory}{publicKeyName}";

        public LicenseAdmin()
        {
            Initialize();
        }

        public override void Initialize()
        {
            if (File.Exists(secretKeyPath))
            {
                var key = File.ReadAllBytes(secretKeyPath);
                cryptoProvider.ImportCspBlob(key);
                SecretKeyLoaded = true;
            }
        }

        public static void NewSignKeyPair()
        {
            new FileInfo(DefaultDirectory).Directory.Create();          

            using (var rsaProvider = new RSACryptoServiceProvider(KeySize))
            {
                var secretKey = rsaProvider.ExportCspBlob(true);
                File.WriteAllBytes(secretKeyPath, secretKey);

                var publicKey = rsaProvider.ExportCspBlob(false);
                File.WriteAllBytes(publicKeyPath, publicKey);
            }
        }

        public void SaveLicense(License license, string licensePath)
        {
            if (!SecretKeyLoaded)
                throw new InvalidOperationException("Ошибка генерации лицензии: отсутствует закрытый ключ.");

            var signPath = Path.ChangeExtension(licensePath, SignExtension);

            using (var licenseStream = new FileStream(licensePath, FileMode.CreateNew))
            using (var signStream = new FileStream(signPath, FileMode.CreateNew))
            {
                var formatter = new XmlSerializer(license.GetType());

                formatter.Serialize(licenseStream, license);

                licenseStream.Position = 0;

                var licenseSign = cryptoProvider.SignData(licenseStream, new SHA512CryptoServiceProvider());

                signStream.Write(licenseSign, 0, licenseSign.Length);
            }
        }
    }
}
