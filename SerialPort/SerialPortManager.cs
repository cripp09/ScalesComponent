using ScalesComponent.Models;
using ScalesComponent.Parsers;

using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScalesComponent.SerialPort
{
    /// <summary>
    /// Менеджер последовательного порта с фоновым чтением данных
    /// </summary>
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

        public SerialPortManager(IScalesParser parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Подключение к COM-порту
        /// </summary>
        public void Connect(string portName, int baudRate = 9600)
        {
            Disconnect(); // Закрываем предыдущее соединение если есть

            _serialPort = new System.IO.Ports.SerialPort(portName, baudRate)
            {
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500,
                Encoding = Encoding.UTF8
            };

            _serialPort.Open();
            _incomingBuffer.Clear();

            // Запускаем фоновый поток чтения
            _cancellationTokenSource = new CancellationTokenSource();
            _backgroundReadTask = Task.Run(() => ReadLoop(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Фоновый цикл чтения данных из порта
        /// </summary>
        private void ReadLoop(CancellationToken token)
        {
            byte[] buffer = new byte[1024];

            while (!token.IsCancellationRequested && _serialPort?.IsOpen == true)
            {
                try
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);
                        string chunk = _serialPort.Encoding.GetString(buffer, 0, bytesRead);

                        lock (_incomingBuffer)
                        {
                            _incomingBuffer.Append(chunk);
                        }

                        ProcessBuffer();
                    }
                    else
                    {
                        Thread.Sleep(50); // Пауза чтобы не грузить CPU
                    }
                }
                catch (TimeoutException)
                {
                    // Игнорируем таймауты - это нормально
                }
                catch (Exception)
                {
                    // При ошибке выходим из цикла
                    break;
                }
            }
        }

        /// <summary>
        /// Обработка буфера входящих данных
        /// </summary>
        private void ProcessBuffer()
        {
            string bufferCopy;
            lock (_incomingBuffer)
            {
                bufferCopy = _incomingBuffer.ToString();
            }

            // Обрабатываем построчно
            while (bufferCopy.Contains('\n'))
            {
                int index = bufferCopy.IndexOf('\n');
                string line = bufferCopy.Substring(0, index).Trim();
                bufferCopy = bufferCopy.Substring(index + 1);

                if (!string.IsNullOrEmpty(line))
                {
                    var reading = _parser.Parse(line);
                    if (reading != null)
                    {
                        lock (_lockObj)
                        {
                            _lastReading = reading;
                        }
                    }
                }
            }

            // Очищаем обработанные данные из буфера
            lock (_incomingBuffer)
            {
                _incomingBuffer.Clear();
                _incomingBuffer.Append(bufferCopy);
            }
        }

        /// <summary>
        /// Получить последнее чтение с весов
        /// </summary>
        public ScalesReading GetLastReading()
        {
            lock (_lockObj)
            {
                return _lastReading;
            }
        }

        /// <summary>
        /// Отправить команду весам
        /// </summary>
        public void SendCommand(string command)
        {
            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Write(command);
            }
        }

        /// <summary>
        /// Отключение от порта
        /// </summary>
        public void Disconnect()
        {
            _cancellationTokenSource?.Cancel();

            try
            {
                _backgroundReadTask?.Wait(1000);
            }
            catch { }

            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}