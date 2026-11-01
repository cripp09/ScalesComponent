using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Bizerba (Германия)
    /// Протокол: "S 12.345 kg" или "US 12.345 kg"
    /// Скорость: 9600, 8N1
    /// </summary>
    /// <remarks>
    /// ⚠️ ВНИМАНИЕ: Этот парсер реализован на основе официальной документации 
    /// производителя и НЕ был физически протестирован на реальном оборудовании.
    /// Перед использованием в промышленной эксплуатации рекомендуется провести 
    /// верификацию на вашей конкретной модели весов.
    /// См. TESTING_STATUS.md для подробностей.
    /// </remarks>
    public class BizerbaParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var match = Regex.Match(line, @"^\s*([US]?S)\s+([+-]?\d+\.?\d*)\s*(kg|g)?");

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
        public string GetBrandName() => "Bizerba";
    }
}