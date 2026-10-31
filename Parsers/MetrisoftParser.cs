using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Metrisoft
    /// Формат данных: A/B=0.6S (где S = stable)
    /// </summary>
    public class MetrisoftParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            // Формат: A/B=0.6S или A/B=0.6
            var match = Regex.Match(line, @"A/B=\s*(-?\d+\.?\d*)S?");

            if (match.Success)
            {
                // Используем InvariantCulture для корректного парсинга точки как разделителя
                if (double.TryParse(match.Groups[1].Value, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double weight))
                {
                    bool isStable = line.Contains("S");

                    return new ScalesReading
                    {
                        Weight = Math.Round(weight, 3),
                        IsStable = isStable,
                        RawData = line,
                        LastUpdate = DateTime.Now
                    };
                }
            }

            return null;
        }

        public string GetDetectionPattern()
        {
            return @"A/B=\s*-?\d+\.?\d*S?";
        }
        public string GetBrandName() => "Metrisoft";
    }
}