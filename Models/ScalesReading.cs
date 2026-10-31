using System;

namespace ScalesComponent.Models
{
    /// <summary>
    /// Модель данных чтения с весов
    /// </summary>
    public class ScalesReading
    {
        /// <summary>
        /// Вес в килограммах
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Признак стабильности веса (флаг S от весов)
        /// </summary>
        public bool IsStable { get; set; }

        /// <summary>
        /// Исходные данные от весов (для отладки)
        /// </summary>
        public string RawData { get; set; }

        /// <summary>
        /// Время последнего обновления
        /// </summary>
        public DateTime LastUpdate { get; set; }

        public override string ToString()
        {
            return $"{Weight} kg (Stable: {IsStable})";
        }
    }
}