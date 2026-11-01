using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Sartorius (Германия)
    /// Команда: "S" - запрос стабильного веса
    /// Ответ: "S S 12.345 g" (первый S = команда, второй S = stable)
    /// Скорость: 9600, 8N1
    /// </summary>
    /// <remarks>
    /// ⚠️ ВНИМАНИЕ: Этот парсер реализован на основе официальной документации 
    /// производителя и НЕ был физически протестирован на реальном оборудовании.
    /// Перед использованием в промышленной эксплуатации рекомендуется провести 
    /// верификацию на вашей конкретной модели весов.
    /// См. TESTING_STATUS.md для подробностей.
    /// </remarks>
    public class SartoriusParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var match = Regex.Match(line, @"S\s+([US]?S)\s+([+-]?\d+\.?\d*)\s*(g|kg)?");

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

        public string GetDetectionPattern() => @"^S\s+[US]?S\s+[+-]?\d+";
        public string GetBrandName() => "Sartorius";
    }
}