using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Digi SM-100/SM-300 (Япония)
    /// Команда: "@00W" - запрос веса
    /// Ответ: "@00W 12.345"
    /// Скорость: 9600, 8N1
    /// </summary>
    public class DigiParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var match = Regex.Match(line, @"@\d+W\s+([+-]?\d+\.?\d*)");

            if (match.Success)
            {
                if (double.TryParse(match.Groups[1].Value,
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                {
                    return new ScalesReading
                    {
                        Weight = Math.Round(weight, 3),
                        IsStable = true, // Digi всегда шлёт стабильный вес
                        RawData = line,
                        LastUpdate = DateTime.Now
                    };
                }
            }
            return null;
        }

        public string GetDetectionPattern() => @"@\d+W\s+[+-]?\d+";
        public string GetBrandName() => "Digi";
    }
}