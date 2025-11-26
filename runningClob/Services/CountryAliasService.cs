namespace runningClob.Services
{
    public interface ICountryAliasService
    {
        string NormalizeCountry(string countryInput);
        string GetCountryCode(string countryInput);
        List<string> GetCountryAliases(string countryCode);
        public List<string> GetCommonCountryNames();
    }

    public class CountryAliasService : ICountryAliasService
    {
        private readonly Dictionary<string, string> _countryAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // North America
            { "US", "US" }, { "USA", "US" }, { "United States", "US" }, { "America", "US" },
            { "CA", "CA" }, { "Canada", "CA" },
            { "MX", "MX" }, { "Mexico", "MX" },
            
            // Europe
            { "GB", "GB" }, { "UK", "GB" }, { "United Kingdom", "GB" }, { "Britain", "GB" },
            { "DE", "DE" }, { "Germany", "DE" }, { "Deutschland", "DE" },
            { "FR", "FR" }, { "France", "FR" },
            { "ES", "ES" }, { "Spain", "ES" }, { "España", "ES" },
            { "IT", "IT" }, { "Italy", "IT" }, { "Italia", "IT" },
            
            // Middle East / Arabian Nations
            { "EG", "EG" }, { "Egypt", "EG" }, { "Arab Republic of Egypt", "EG" },
            { "SA", "SA" }, { "Saudi Arabia", "SA" }, { "KSA", "SA" },
            { "AE", "AE" }, { "United Arab Emirates", "AE" }, { "UAE", "AE" }, { "Emirates", "AE" },
            { "QA", "QA" }, { "Qatar", "QA" },
            { "KW", "KW" }, { "Kuwait", "KW" },
            { "BH", "BH" }, { "Bahrain", "BH" },
            { "OM", "OM" }, { "Oman", "OM" },
            { "JO", "JO" }, { "Jordan", "JO" },
            { "LB", "LB" }, { "Lebanon", "LB" },
            
            // Add other regions as needed...
        };

        private readonly Dictionary<string, List<string>> _countryToAliases = new()
        {
            { "US", new List<string> { "United States", "USA", "America", "U.S." } },
            { "GB", new List<string> { "United Kingdom", "UK", "Great Britain" } },
            { "CA", new List<string> { "Canada", "CAN" } },
            { "AU", new List<string> { "Australia", "AUS" } },
            { "EG", new List<string> { "Egypt", "Arab Republic of Egypt" } },
            { "SA", new List<string> { "Saudi Arabia", "KSA" } },
            { "AE", new List<string> { "United Arab Emirates", "UAE", "Emirates" } }
        };

        public string NormalizeCountry(string countryInput)
        {
            if (string.IsNullOrWhiteSpace(countryInput))
                return null;

            return _countryAliases.TryGetValue(countryInput.Trim(), out var normalized)
                ? normalized
                : countryInput.Trim().ToUpper();
        }

        public string GetCountryCode(string countryInput)
        {
            return NormalizeCountry(countryInput);
        }

        public List<string> GetCountryAliases(string countryCode)
        {
            return _countryToAliases.TryGetValue(countryCode, out var aliases)
                ? aliases
                : new List<string> { countryCode };
        }
        public List<string> GetCommonCountryNames()
        {
            return _countryToAliases
                .SelectMany(x => x.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}