using ScalesComponent.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScalesComponent.Parsers
{
    public class AutoDetectParser : IScalesParser
    {
        private readonly Dictionary<string, IScalesParser> _parsers;
        private IScalesParser _detectedParser;
        private int _samplesCollected;
        private readonly Dictionary<string, int> _matchCounts;

        public AutoDetectParser()
        {
            _parsers = new Dictionary<string, IScalesParser>(StringComparer.OrdinalIgnoreCase)
            {
                { "Metrisoft", new MetrisoftParser() },
                { "CAS", new CasParser() },
                { "MassaK", new MassaKParser() },
                { "MettlerToledo", new MettlerToledoParser() },
                { "A&D", new AndParser() },
                { "Ohaus", new OhausParser() },
                { "Shtrih", new ShtrihParser() },
                { "Digi", new DigiParser() },
                { "Rys", new RysParser() },
                { "Vesta", new VestaParser() },
                { "Bizerba", new BizerbaParser() },
                { "Sartorius", new SartoriusParser() },
                { "Radwag", new RadwagParser() },
                { "Mercury", new MercuryParser() },
                { "TenzoM", new TenzoParser() }
            };
            
            _matchCounts = new Dictionary<string, int>();
        }

        public ScalesReading Parse(string line)
        {
            if (_detectedParser != null)
            {
                return _detectedParser.Parse(line);
            }

            // Режим автоопределения: собираем статистику
            _samplesCollected++;
            
            foreach (var kvp in _parsers)
            {
                string pattern = kvp.Value.GetDetectionPattern();
                if (Regex.IsMatch(line, pattern))
                {
                    if (!_matchCounts.ContainsKey(kvp.Key))
                        _matchCounts[kvp.Key] = 0;
                    _matchCounts[kvp.Key]++;
                }
            }

            // Определяем тип после 3 совпадений
            if (_samplesCollected >= 3)
            {
                if (_matchCounts.Count > 0)
                {
                    string bestMatch = _matchCounts.OrderByDescending(x => x.Value).First().Key;
                    _detectedParser = _parsers[bestMatch];
                }
            }

            return null;
        }

        public string GetDetectionPattern() => null;
        public string GetBrandName() => "AutoDetect";

        public string GetDetectedType()
        {
            foreach (var kvp in _parsers)
            {
                if (kvp.Value == _detectedParser)
                    return kvp.Key;
            }
            return null;
        }

        public List<string> GetAvailableTypes()
        {
            return new List<string>(_parsers.Keys);
        }
    }
}