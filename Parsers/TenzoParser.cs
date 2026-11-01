using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов Тензо-М (Россия)
    /// ASCII протокол
    /// Ответ: "12.345 kg" или "W=12.345"
    /// Скорость: 9600, 8N1
    /// </summary>
    /// <remarks>
    /// ⚠️ ВНИМАНИЕ: Этот парсер реализован на основе официальной документации 
    /// производителя и НЕ был физически протестирован на реальном оборудовании.
    /// Перед использованием в промышленной эксплуатации рекомендуется провести 
    /// верификацию на вашей конкретной модели весов.
    /// См. TESTING_STATUS.md для подробностей.
    /// </remarks>
    public class TenzoParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            // Формат: "12.345 kg" или "W=12.345"
            var match = Regex.Match(line, @"(?:W=)?\s*([+-]?\d+\.?\d*)\s*(kg|g)?");

            if (match.Success)
            {
                if (double.TryParse(match.Groups[1].Value,
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

        public string GetDetectionPattern() => @"(?:W=)?\s*[+-]?\d+\.\d+";
        public string GetBrandName() => "TenzoM";
    }
}