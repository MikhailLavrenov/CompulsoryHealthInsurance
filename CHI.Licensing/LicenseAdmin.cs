﻿using CHI.Infrastructure;
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
        static readonly int KeySize = 2048;
        static readonly string secretKeyName = "licensing.skey";
        bool SecretKeyLoaded;


        /// <summary>
        /// Полный путь к ключевой паре подписи лицензий.
        /// </summary>
        internal string SecretKeyPath { get; }
        /// <summary>
        /// Полный путь к открытому ключу проверки подписи лицензий.
        /// </summary>
        internal string PublicKeyPath { get; }


        /// <summary>
        /// Конструктор. Вызывает инициализацию класса.
        /// </summary>
        internal LicenseAdmin()
        {
            PublicKeyPath = Path.Combine(DefaultDirectory, publicKeyName);
            SecretKeyPath = Path.Combine(DefaultDirectory, secretKeyName);

            Initialize();
        }


        /// <summary>
        /// Инициализирует класс: загружает ключевую пару подписи лицензий.
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
        internal void NewSignKeyPair()
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
        internal void SignAndSaveLicense(License license, string licensePath)
        {
            if (!SecretKeyLoaded)
                throw new InvalidOperationException("Ошибка генерации лицензии: отсутствует закрытый ключ.");
            
            var signedLicense = new SignedLicense();
            signedLicense.License = license;

            var licenseStream = new MemoryStream();
            var formatter = new XmlSerializer(license.GetType());
            formatter.Serialize(licenseStream, license);
            licenseStream.Position = 0;

            signedLicense.Sign = cryptoProvider.SignData(licenseStream, SHA512.Create());

            using var stream = new FileStream(licensePath, FileMode.CreateNew);
            formatter = new XmlSerializer(signedLicense.GetType());
            formatter.Serialize(stream, signedLicense);
        }
    }
}
