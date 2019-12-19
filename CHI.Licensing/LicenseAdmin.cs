using CHI.Application;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace CHI.Licensing
{
    /// <summary>
    /// Представляет средства управления лицензированием приложения.
    /// </summary>
    internal sealed class LicenseAdmin : LicenseManager
    {
        private static readonly int KeySize = 2048;
        private static readonly string secretKeyName = "licensing.skey";
        private bool SecretKeyLoaded;

        /// <summary>
        /// Полный путь к ключевой паре подписи лицензий.
        /// </summary>
        internal static string SecretKeyPath { get; } = $"{DefaultDirectory}{secretKeyName}";
        /// <summary>
        /// Полный путь к открытому ключу проверки подписи лицензий.
        /// </summary>
        internal static string PublicKeyPath { get; } = $"{DefaultDirectory}{publicKeyName}";

        /// <summary>
        /// Конструктор. Вызывает инициализацию класса.
        /// </summary>
        internal LicenseAdmin()
        {
            Initialize();
        }

        /// <summary>
        /// Инициализирует класс: загржуает ключевую пару подписи лицензий.
        /// </summary>
        public override void Initialize()
        {
            if (File.Exists(SecretKeyPath))
            {
                var key = File.ReadAllBytes(SecretKeyPath);
                cryptoProvider.ImportCspBlob(key);
                SecretKeyLoaded = true;
            }
        }
        /// <summary>
        /// Генерирует и сохраняет в файлы новую ключевую пару для подписи лицензий.
        /// </summary>
        internal static void NewSignKeyPair()
        {
            new FileInfo(DefaultDirectory).Directory.Create();

            using (var rsaProvider = new RSACryptoServiceProvider(KeySize))
            {
                var secretKey = rsaProvider.ExportCspBlob(true);
                File.WriteAllBytes(SecretKeyPath, secretKey);

                var publicKey = rsaProvider.ExportCspBlob(false);
                File.WriteAllBytes(PublicKeyPath, publicKey);
            }
        }        
        /// <summary>
        /// Сохраняет лицензию в файл, генерирует его подпись и сохраняет подпись в файл по тому же пути и имени но с отличным расширением.
        /// </summary>
        /// <param name="license">Лицензия</param>
        /// <param name="licensePath">Полный путь для сохранения лицензии.</param>
        /// <exception cref="InvalidOperationException">Возникает когда не найден закрытый ключ для подписания лицензий.</exception>
        internal void SingAndSaveLicense(License license, string licensePath)
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
