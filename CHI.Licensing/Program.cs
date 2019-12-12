using System;
using System.IO;

namespace CHI.Licensing
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Менеджер ключей лицензирования");
                Console.WriteLine("-----------------------------");
                Console.WriteLine();
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Создать новую ключевую пару");
                Console.WriteLine("0. Выход");

                var pressedKey = Console.ReadKey(true);

                if (pressedKey.KeyChar == '1')
                {
                    if (File.Exists(LicenseManager.secretKeyPath))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Текущий ключ лицензирования будет удален, продолжить?");
                        Console.WriteLine("1. Да");
                        Console.WriteLine("2. Нет");

                        pressedKey = Console.ReadKey(true);

                        if (pressedKey.KeyChar == '2')
                            continue;
                    }

                    LicenseManager.GenerateNewKeyPair();
                    Console.WriteLine();
                    Console.WriteLine("Создан новый ключ лицензирования");
                }
                else if (pressedKey.KeyChar == '0')
                    Environment.Exit(0);
            }

        }
    }
}
