using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Меркурий (Россия)
    /// Простой ASCII протокол
    /// Ответ: "12.345" или "+12.345"
    /// Скорость: 9600, 8N1
    /// </summary>
    public class MercuryParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var match = Regex.Match(line.Trim(), @"^[+-]?\d+\.?\d*$");

            if (match.Success)
            {
                if (double.TryParse(match.Value,
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                {
                    return new ScalesReading
                    {
                        Weight = Math.Round(weight, 3),
                        IsStable = true,
                        RawData = line,
                        LastUpdate = DateTime.Now
                    };
                }
            }
            return null;
        }

        public string GetDetectionPattern() => @"^[+-]?\d+\.\d+$";
        public string GetBrandName() => "Mercury";
    }
}