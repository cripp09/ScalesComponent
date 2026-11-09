using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using ScalesComponent.Models;
using ScalesComponent.Parsers;
using ScalesComponent.SerialPort;

namespace ScalesComponent
{
    /// <summary>
    /// COM-объект для работы с весами из 1С
    /// </summary>
    [ComVisible(true)]
    [Guid("B2C3D4E5-F6A7-8901-BCDE-F12345678901")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(IScales))]
    [ProgId("ScalesComponent.Scales.1")]
    public class Scales : IScales, IDisposable
    {
        private SerialPortManager _portManager;
        private string _lastError = "";
        private IScalesParser _currentParser;

        public Scales()
        {
            _lastError = "";
        }

        /// <summary>
        /// Подключение к весам
        /// </summary>
        public bool Connect(string portName, int baudRate, string scalesType)
        {
            try
            {
                _lastError = "";

                // Создаем парсер в зависимости от типа весов
                _currentParser = CreateParser(scalesType);

                if (_currentParser == null)
                {
                    _lastError = $"Неизвестный тип весов: {scalesType}";
                    return false;
                }

                _portManager = new SerialPortManager(_currentParser);
                _portManager.Connect(portName, baudRate);

                return true;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Отключение от весов
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _portManager?.Disconnect();
                _lastError = "";
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
            }
        }

        /// <summary>
        /// Получить текущий вес (мгновенно)
        /// </summary>
        public double GetCurrentWeight()
        {
            try
            {
                var reading = _portManager?.GetLastReading();

                // Если кэш пустой, ждём первое значение (до 1 секунды)
                if (reading == null)
                {
                    int elapsed = 0;
                    while (elapsed < 1000)
                    {
                        reading = _portManager?.GetLastReading();
                        if (reading != null) break;

                        System.Threading.Thread.Sleep(50);
                        elapsed += 50;
                    }
                }

                return reading?.Weight ?? 0.0;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                return 0.0;
            }
        }

        /// <summary>
        /// Получить стабильный вес с ожиданием
        /// </summary>
        public double GetStableWeight(int timeoutMs)
        {
            try
            {
                int elapsed = 0;
                int step = 100; // Проверяем каждые 100 мс

                while (elapsed < timeoutMs)
                {
                    var reading = _portManager?.GetLastReading();
                    if (reading != null && reading.IsStable)
                    {
                        return reading.Weight;
                    }

                    Thread.Sleep(step);
                    elapsed += step;
                }

                _lastError = "Не удалось получить стабильный вес за отведенное время";
                return 0.0;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                return 0.0;
            }
        }

        /// <summary>
        /// Проверка подключения
        /// </summary>
        public bool IsConnected()
        {
            return _portManager?.IsConnected ?? false;
        }

        /// <summary>
        /// Проверка стабильности веса
        /// </summary>
        public bool IsStable()
        {
            var reading = _portManager?.GetLastReading();
            return reading?.IsStable ?? false;
        }

        /// <summary>
        /// Тарирование весов
        /// </summary>
        public void Tare()
        {
            try
            {
                if (_currentParser is MetrisoftParser)
                {
                    // Команда тарирования для Metrisoft
                    _portManager?.SendCommand("XA/TA\r\n");
                }
                // Можно добавить команды тарирования для других производителей
                _lastError = "";
            }
            catch (Exception ex)
            {
                _lastError = $"Ошибка тарирования: {ex.Message}";
            }
        }

        /// <summary>
        /// Получить последнюю ошибку
        /// </summary>
        public string GetLastError()
        {
            return _lastError;
        }

        /// <summary>
        /// Получить список доступных типов весов
        /// </summary>
        public string[] GetAvailableScalesTypes()
        {
            return new[]
            {
                "Metrisoft", "CAS", "MassaK", "MettlerToledo", "A&D",
                "Ohaus", "Shtrih", "Digi", "Rys", "Vesta",
                "Bizerba", "Sartorius", "Radwag", "Mercury", "TenzoM",
                "Auto"
            };
        }

        /// <summary>
        /// Создать парсер по типу весов
        /// </summary>
        private IScalesParser CreateParser(string scalesType)
        {
            switch (scalesType.ToUpper())
            {
                case "METRISOFT": return new MetrisoftParser();
                case "CAS": return new CasParser();
                case "MASSAK": return new MassaKParser();
                case "MASSA-K":return new MassaKParser();
                case "METTLERTOLEDO": return new MettlerToledoParser();
                case "A&D": case "AND": return new AndParser();
                case "OHAUS": return new OhausParser();
                case "SHTRIH": return new ShtrihParser();
                case "DIGI": return new DigiParser();
                case "RYS": return new RysParser();
                case "VESTA": return new VestaParser();
                case "BIZERBA": return new BizerbaParser();
                case "SARTORIUS": return new SartoriusParser();
                case "RADWAG": case "KERN": return new RadwagParser();
                case "MERCURY": return new MercuryParser();
                case "TENZOM": return new TenzoParser();
                case "AUTO": return new AutoDetectParser();
                default: return null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}