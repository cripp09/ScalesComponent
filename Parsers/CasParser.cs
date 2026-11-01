using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов CAS (CI-200A, CI-5200, AP-6800 и др.)
    /// 
    /// Реальный формат данных:
    ///   US,NT,▒,   244.5 kg   — нестабильный вес
    ///   ST,GS,▒,   152.0 kg   — стабильный вес брутто
    ///   OL,NT,▒,  1504.5 kg   — ПЕРЕГРУЗ (значение невалидно!)
    /// 
    /// Поля:
    ///   1. Статус: ST (stable), US (unstable), OL (overload)
    ///   2. Тип:    GS (gross), NT (net), TA (tare)
    ///   3. Единица измерения (символ кодировки, часто отображается как ▒)
    ///   4. Значение веса с единицами
    /// 
    /// Настройки COM-порта: 9600, 8N1
    /// </summary>
    public class CasParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            // Убираем символы-артефакты кодировки (▒ и подобные)
            line = Regex.Replace(line, @"[^\x20-\x7E]", "");

            // Формат: STATUS,TYPE,UNIT, VALUE kg
            // Пример: US,NT,,   244.5 kg  или  ST,GS,,   152.0 kg
            var match = Regex.Match(
                line,
                @"(ST|US|OL),\s*(GS|NT|TA),\s*[^,]*,\s*([+-]?\s*\d+\.?\d*)\s*(kg|g|lb)?",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
                return null;

            string status = match.Groups[1].Value.ToUpper();
            string weightStr = match.Groups[3].Value.Replace(" ", "");

            // Перегруз — возвращаем null, чтобы это значение не попало в кэш
            if (status == "OL")
            {
                return new ScalesReading
                {
                    Weight = 0,
                    IsStable = false,
                    RawData = "OVERLOAD: " + line,
                    LastUpdate = DateTime.Now
                };
            }

            if (!double.TryParse(weightStr, NumberStyles.Float,
                CultureInfo.InvariantCulture, out double weight))
                return null;

            return new ScalesReading
            {
                Weight = Math.Round(weight, 3),
                IsStable = status == "ST",
                RawData = line,
                LastUpdate = DateTime.Now
            };
        }

        /// <summary>
        /// Паттерн для автоопределения весов CAS.
        /// Ищем характерную последовательность STATUS,TYPE,
        /// </summary>
        public string GetDetectionPattern()
        {
            return @"(ST|US|OL),\s*(GS|NT|TA),";
        }

        public string GetBrandName() => "CAS";
    }
}