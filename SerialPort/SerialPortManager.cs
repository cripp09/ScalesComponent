using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ScalesComponent.Models;
using ScalesComponent.Parsers;

namespace ScalesComponent.SerialPort
{
    public class SerialPortManager : IDisposable
    {
        private System.IO.Ports.SerialPort _serialPort;
        private readonly StringBuilder _incomingBuffer = new StringBuilder();
        private readonly IScalesParser _parser;

        private Task _backgroundReadTask;
        private CancellationTokenSource _cancellationTokenSource;

        private ScalesReading _lastReading;
        private readonly object _lockObj = new object();

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        // Определяем, является ли текущий парсер Масса-К
        private bool IsMassaK => _parser.GetType().Name == "MassaKParser";

        public SerialPortManager(IScalesParser parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public void Connect(string portName, int baudRate = 9600)
        {
            Disconnect();

            // ТОЧЕЧНАЯ ПРАВКА: Для Масса-К принудительно ставим правильные настройки
            int finalBaud = IsMassaK ? 4800 : baudRate;
            Parity finalParity = IsMassaK ? Parity.Even : Parity.None;

            _serialPort = new System.IO.Ports.SerialPort(portName, finalBaud)
            {
                DataBits = 8,
                Parity = finalParity,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500,
                Encoding = Encoding.ASCII
            };

            _serialPort.Open();

            // Очищаем буферы (критично для запрос-ответ)
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();

            _cancellationTokenSource = new CancellationTokenSource();
            _backgroundReadTask = Task.Run(() => ReadLoop(_cancellationTokenSource.Token));
        }

        private void ReadLoop(CancellationToken token)
        {
            byte[] buffer = new byte[1024];

            while (!token.IsCancellationRequested && _serialPort?.IsOpen == true)
            {
                try
                {
                    // 1. СПЕЦ. ОБРАБОТКА ДЛЯ МАССА-К: отправляем запрос 0x45
                    if (IsMassaK)
                    {
                        _serialPort.DiscardInBuffer(); // Сбрасываем старый "мусор"
                        _serialPort.Write(new byte[] { 0x45 }, 0, 1);
                        Thread.Sleep(50); // Даем весам 50 мс на формирование ответа
                    }

                    // 2. Чтение данных
                    if (_serialPort.BytesToRead > 0)
                    {
                        if (IsMassaK)
                        {
                            // Читаем ровно 2 байта для бинарного протокола Масса-К
                            if (_serialPort.BytesToRead >= 2)
                            {
                                byte[] response = new byte[2];
                                _serialPort.Read(response, 0, 2);

                                // Парсим по спецификации: Little Endian, D15=знак, D14-D0=граммы
                                ushort rawValue = (ushort)(response[0] | (response[1] << 8));
                                bool isNegative = (rawValue & 0x8000) != 0;
                                int massInGrams = rawValue & 0x7FFF;

                                double weightInKg = massInGrams / 1000.0;
                                if (isNegative) weightInKg = -weightInKg;

                                var reading = new ScalesReading
                                {
                                    Weight = Math.Round(weightInKg, 3),
                                    IsStable = true,
                                    RawData = $"HEX: {response[0]:X2} {response[1]:X2} | {weightInKg:F3} кг",
                                    LastUpdate = DateTime.Now
                                };

                                lock (_lockObj) { _lastReading = reading; }
                            }
                        }
                        else
                        {
                            // СТАНДАРТНАЯ обработка для всех остальных 14 брендов (CAS, Mettler и т.д.)
                            int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);
                            string chunk = _serialPort.Encoding.GetString(buffer, 0, bytesRead);

                            lock (_incomingBuffer) { _incomingBuffer.Append(chunk); }
                            ProcessBuffer();
                        }
                    }
                    else
                    {
                        Thread.Sleep(30); // Пауза, чтобы не грузить CPU
                    }
                }
                catch (TimeoutException) { }
                catch (Exception) { break; }
            }
        }

        private void ProcessBuffer()
        {
            while (true)
            {
                string line = null;
                lock (_incomingBuffer)
                {
                    string currentContent = _incomingBuffer.ToString();
                    int index = currentContent.IndexOf('\n');
                    if (index < 0) index = currentContent.IndexOf('\r');

                    if (index >= 0)
                    {
                        line = currentContent.Substring(0, index).Trim();
                        _incomingBuffer.Remove(0, index + 1);
                    }
                }

                if (line == null) break;

                if (!string.IsNullOrEmpty(line))
                {
                    var reading = _parser.Parse(line);
                    if (reading != null)
                    {
                        lock (_lockObj) { _lastReading = reading; }
                    }
                }
            }
        }

        public ScalesReading GetLastReading()
        {
            lock (_lockObj) { return _lastReading; }
        }

        public void SendCommand(string command)
        {
            if (_serialPort?.IsOpen == true) _serialPort.Write(command);
        }

        public void Disconnect()
        {
            _cancellationTokenSource?.Cancel();
            try { _backgroundReadTask?.Wait(1000); } catch { }

            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        public void Dispose() => Disconnect();
    }
}