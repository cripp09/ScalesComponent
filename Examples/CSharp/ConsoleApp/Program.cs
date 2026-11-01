using System;
using System.Threading;
using ScalesComponent;

namespace ScalesComponent.Examples.ConsoleApp
{
    /// <summary>
    /// Пример консольного приложения для работы с весами через ScalesComponent.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  ScalesComponent - Консольный пример использования        ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Параметры подключения (настройте под ваше оборудование)
            string portName = "COM1";
            int baudRate = 9600;
            string scalesType = "CAS"; // Автоматическое определение типа весов

            Console.WriteLine($"Параметры подключения:");
            Console.WriteLine($"  COM-порт: {portName}");
            Console.WriteLine($"  Скорость: {baudRate}");
            Console.WriteLine($"  Тип весов: {scalesType}");
            Console.WriteLine();

            // Создаём объект весов
            using (var scales = new Scales())
            {
                // Подключение к весам
                Console.Write("Подключение к весам... ");
                bool connected = scales.Connect(portName, baudRate, scalesType);

                if (!connected)
                {
                    Console.WriteLine("ОШИБКА!");
                    Console.WriteLine($"  {scales.GetLastError()}");
                    Console.WriteLine();
                    Console.WriteLine("Проверьте:");
                    Console.WriteLine("  1. Весы включены и подключены к ПК");
                    Console.WriteLine("  2. COM-порт не занят другим приложением");
                    Console.WriteLine("  3. Настройки COM-порта соответствуют весам");
                    Console.WriteLine();
                    Console.WriteLine("Нажмите любую клавишу для выхода...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("УСПЕШНО!");
                Console.WriteLine();

                // Основное меню
                while (true)
                {
                    Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║  МЕНЮ                                                     ║");
                    Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
                    Console.WriteLine("  [1] Получить стабильный вес (с ожиданием)");
                    Console.WriteLine("  [2] Получить текущий вес (мгновенно)");
                    Console.WriteLine("  [3] Тарировать весы");
                    Console.WriteLine("  [4] Проверить статус подключения");
                    Console.WriteLine("  [5] Проверить стабильность веса");
                    Console.WriteLine("  [0] Отключиться и выйти");
                    Console.WriteLine();
                    Console.Write("Выберите действие: ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            GetStableWeight(scales);
                            break;
                        case "2":
                            GetCurrentWeight(scales);
                            break;
                        case "3":
                            Tare(scales);
                            break;
                        case "4":
                            CheckConnectionStatus(scales);
                            break;
                        case "5":
                            CheckStability(scales);
                            break;
                        case "0":
                            Console.WriteLine("Отключение от весов...");
                            scales.Disconnect();
                            Console.WriteLine("Готово. До свидания!");
                            return;
                        default:
                            Console.WriteLine("Неверный выбор. Попробуйте снова.");
                            break;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        /// <summary>
        /// Получить стабильный вес с ожиданием.
        /// </summary>
        static void GetStableWeight(Scales scales)
        {
            Console.Write("Ожидание стабильного веса (до 5 секунд)... ");

            try
            {
                double weight = scales.GetStableWeight(5000);
                Console.WriteLine($"Вес: {weight:F3} кг");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить текущий вес мгновенно.
        /// </summary>
        static void GetCurrentWeight(Scales scales)
        {
            try
            {
                double weight = scales.GetCurrentWeight();
                Console.WriteLine($"Текущий вес: {weight:F3} кг");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex.Message}");
            }
        }

        /// <summary>
        /// Тарировать весы.
        /// </summary>
        static void Tare(Scales scales)
        {
            Console.Write("Тарирование... ");

            try
            {
                scales.Tare();
                Console.WriteLine("УСПЕШНО!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверить статус подключения.
        /// </summary>
        static void CheckConnectionStatus(Scales scales)
        {
            bool isConnected = scales.IsConnected();
            Console.WriteLine($"Статус подключения: {(isConnected ? "Подключено" : "Отключено")}");
        }

        /// <summary>
        /// Проверить стабильность веса.
        /// </summary>
        static void CheckStability(Scales scales)
        {
            bool isStable = scales.IsStable();
            Console.WriteLine($"Стабильность веса: {(isStable ? "Стабильный" : "Нестабильный")}");
        }
    }
}