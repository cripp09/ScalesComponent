using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Масса-К (Россия)
    /// Протокол МК: команда "D" - запрос данных
    /// Ответ: "S 001234" (stable) или "D 001234" (dynamic)
    /// Скорость: 2400/9600, 8N1
    /// </summary>
    public class MassaKParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            // Формат: "S 001234" или "D 001234" или "S+0012.34"
            var match = Regex.Match(line, @"([SD])\s*[+-]?(\d+\.?\d*)");

            if (match.Success)
            {
                string mode = match.Groups[1].Value;
                if (double.TryParse(match.Groups[2].Value,
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                {
                    return new ScalesReading
                    {
                        Weight = Math.Round(weight, 3),
                        IsStable = mode == "S",
                        RawData = line,
                        LastUpdate = DateTime.Now
                    };
                }
            }
            return null;
        }

        public string GetDetectionPattern() => @"^[SD]\s*\d+";
        public string GetBrandName() => "MassaK";
    }
}