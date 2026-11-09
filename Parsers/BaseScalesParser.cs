using System.IO.Ports;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Базовый класс для всех парсеров. 
    /// Содержит стандартные настройки COM-порта (9600, None), 
    /// которые можно переопределить в конкретных парсерах (например, для Масса-К).
    /// </summary>
    public abstract class BaseScalesParser : IScalesParser
    {
        public abstract ScalesReading Parse(string line);

        public virtual ScalesReading ParseBytes(byte[] data) => null;

        public abstract string GetDetectionPattern();
        public abstract string GetBrandName();

        public virtual bool RequiresPolling => false;
        public virtual byte[] GetPollCommand() => null;
        public virtual int GetDefaultBaudRate() => 9600;
        public virtual Parity GetDefaultParity() => Parity.None;
    }
}