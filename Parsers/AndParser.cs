using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов A&D (Япония)
    /// Команда запроса: "Q"
    /// Ответ: "ST,GS,+0012.345kg" или "US,GS,+0012.345kg"
    /// Скорость: 2400/9600, 8N1
    /// </summary>
    public class AndParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            // Формат: "ST,GS,+0012.345kg"
            var match = Regex.Match(line, @"(ST|US),\s*(GS|NT|TA),\s*([+-]?\d+\.?\d*)\s*(kg|g)?");

            if (match.Success)
            {
                string stability = match.Groups[1].Value;
                if (double.TryParse(match.Groups[3].Value,
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                {
                    return new ScalesReading
                    {
                        Weight = Math.Round(weight, 3),
                        IsStable = stability == "ST",
                        RawData = line,
                        LastUpdate = DateTime.Now
                    };
                }
            }
            return null;
        }

        public string GetDetectionPattern() => @"(ST|US),\s*(GS|NT|TA),\s*[+-]?\d+";
        public string GetBrandName() => "A&D";
    }
}