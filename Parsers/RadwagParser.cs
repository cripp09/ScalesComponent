using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Radwag (Польша) и Kern (Германия)
    /// Протокол: "MASS: 12.345 g" или "MASS_STABLE: 12.345 g"
    /// Скорость: 9600, 8N1
    /// </summary>
    public class RadwagParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var match = Regex.Match(line, @"MASS(?:_STABLE)?:\s*([+-]?\d+\.?\d*)\s*(g|kg)?");

            if (match.Success)
            {
                bool isStable = line.Contains("MASS_STABLE");
                if (double.TryParse(match.Groups[1].Value,
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                {
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

        public string GetDetectionPattern() => @"MASS(?:_STABLE)?:\s*[+-]?\d+";
        public string GetBrandName() => "Radwag";
    }
}