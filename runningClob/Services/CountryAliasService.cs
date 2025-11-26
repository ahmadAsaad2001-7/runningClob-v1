namespace runningClob.Services
{
    public interface ICountryAliasService
    {
        string NormalizeCountry(string countryInput);
        string GetCountryCode(string countryInput);
        List<string> GetCountryAliases(string countryCode);
    }

    public class CountryAliasService : ICountryAliasService
    {
        private readonly Dictionary<string, string> _countryAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // US Aliases
        { "US", "US" }, { "USA", "US" }, { "United States", "US" }, { "United States of America", "US" },
        { "America", "US" }, { "U.S.", "US" }, { "U.S.A.", "US" },
        
        // UK Aliases
        { "GB", "GB" }, { "UK", "GB" }, { "United Kingdom", "GB" }, { "Great Britain", "GB" },
        { "England", "GB" }, { "Scotland", "GB" }, { "Wales", "GB" }, { "Northern Ireland", "GB" },
        
        // Canada
        { "CA", "CA" }, { "Canada", "CA" }, { "CAN", "CA" },
        
        // Australia
        { "AU", "AU" }, { "Australia", "AU" }, { "AUS", "AU" },
        
        // Add more as needed...
    };

        private readonly Dictionary<string, List<string>> _countryToAliases = new()
    {
        { "US", new List<string> { "United States", "USA", "America", "U.S." } },
        { "GB", new List<string> { "United Kingdom", "UK", "Great Britain" } },
        { "CA", new List<string> { "Canada", "CAN" } },
        { "AU", new List<string> { "Australia", "AUS" } }
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
    }
}