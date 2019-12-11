using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace CHI.Licensing
{
    public class Licensor
    {
        private static readonly int KeySize = 2048;
        private static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        private RSACryptoServiceProvider rsaProvider;
        private static readonly string secretKeyPath = $"{KeysDirectory}licensing.skey";
        private static readonly string publicKeyPath = $"{KeysDirectory}licensing.pkey";

        public static string KeysDirectory { get; } = $@"{Directory.GetCurrentDirectory()}\Licensing\";               

        public Licensor()
        {
            string key;

            if (File.Exists(secretKeyPath))
                key=File.ReadAllText(secretKeyPath);
            else if (File.Exists(publicKeyPath))         
                key = File.ReadAllText(publicKeyPath);            
            else
                throw new InvalidOperationException("Ошибка инициализации подсистемы лицензирования: не найден ключ шифрования.");

            rsaProvider.FromXmlString(key);
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

        public void GenerateLicense<T>(T obj)
        {




        }
    }
}
