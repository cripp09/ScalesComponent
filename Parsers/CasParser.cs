using System;
using System.Globalization;
using System.Text.RegularExpressions;

using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Парсер для весов CAS (Южная Корея)
    /// Протокол: ASCII, формат "ST,GS,+ 1.234 kg\r\n"
    /// ST = stable, NT = not stable
    /// GS = gross, NT = net, TA = tare
    /// Скорость: 9600, 8N1
    /// </summary>
    public class CasParser : IScalesParser
    {
        public ScalesReading Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            // Формат: ST,GS,+ 1.234 kg  или  NT,GS,+ 1.234 kg
            var match = Regex.Match(line, @"(ST|NT|US),\s*(GS|NT|TA),\s*([+-]?\s*\d+\.?\d*)\s*(kg|g|lb)?");

            if (match.Success)
            {
                string stability = match.Groups[1].Value;
                if (double.TryParse(match.Groups[3].Value.Replace(" ", ""),
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

        public string GetDetectionPattern() => @"(ST|NT|US),\s*(GS|NT|TA),";
        public string GetBrandName() => "CAS";
    }
}