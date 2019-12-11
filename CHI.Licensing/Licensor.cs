using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace CHI.Licensing
{
    public class Licensor
    {
        private static readonly int KeySize = 2048;
        private static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        private RSACryptoServiceProvider cryptoProvider;
        private readonly bool secretKeyLoaded = false;
        private static readonly string secretKeyPath = $"{KeysDirectory}licensing.skey";
        private static readonly string publicKeyPath = $"{KeysDirectory}licensing.pkey";

        public static string KeysDirectory { get; } = $@"{Directory.GetCurrentDirectory()}\Licensing\";

        public Licensor()
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
                throw new InvalidOperationException("Ошибка инициализации подсистемы лицензирования: не найден ключ шифрования.");

            cryptoProvider.FromXmlString(key);
        }

        public static void GenerateNewKeyPair()
        {
            new FileInfo(KeysDirectory).Directory.Create();
            //var dateTimeStr = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_FFF");

            using (var rsaProvider = new RSACryptoServiceProvider(KeySize))
            {
                var secretKey = rsaProvider.ToXmlString(true);
                File.WriteAllText(secretKeyPath, secretKey);

                var publicKey = rsaProvider.ToXmlString(false);
                File.WriteAllText(publicKeyPath, publicKey);
            }
        }

        public void SaveNewLicense<T>(T obj)
        {
            if (!secretKeyLoaded)
                throw new InvalidOperationException("Ошибка генерации лицензии: отсутствует закрытый ключ.");

            var dateTimeStr = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_FFF");
            var filePath = $"{KeysDirectory}License {nameof(T)} {dateTimeStr}.lic";


            using (var mStream = new MemoryStream())
            using (var fStream = new FileStream(filePath, FileMode.CreateNew))
            {
                var formatter = new XmlSerializer(typeof(T));

                formatter.Serialize(mStream, obj);

                var encryptedBytes = cryptoProvider.Encrypt(mStream.ToArray(), true);

                fStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            }
        }

        public T LoadLicense<T>(string filePath)
        {
            T result;

            using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            using (var mStream = new MemoryStream())
            {
                fStream.CopyTo(mStream);

                var bytes = cryptoProvider.Decrypt(mStream.ToArray(), true);

                var stream = new MemoryStream(bytes);

                var formatter = new XmlSerializer(typeof(T));

                result = (T)formatter.Deserialize(stream);
            }

            return result;

        }
    }
}
