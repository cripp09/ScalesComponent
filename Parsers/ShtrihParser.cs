using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Штрих-М (Россия)
    /// Протокол похож на Mettler Toledo
    /// Ответ: "S 12.345" или "US 12.345"
    /// Скорость: 9600, 8N1
    /// </summary>
    public class ShtrihParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var match = Regex.Match(line, @"^\s*([US]?S)\s+([+-]?\d+\.?\d*)");

            if (match.Success)
            {
                string stability = match.Groups[1].Value;
                if (double.TryParse(match.Groups[2].Value,
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                {
                    return new ScalesReading
                    {
                        Weight = Math.Round(weight, 3),
                        IsStable = stability == "S",
                        RawData = line,
                        LastUpdate = DateTime.Now
                    };
                }
            }
            return null;
        }

        public string GetDetectionPattern() => @"^\s*[US]?S\s+[+-]?\d+";
        public string GetBrandName() => "Shtrih";
    }
}