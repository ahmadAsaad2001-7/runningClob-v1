namespace runningClob.Services
{
    public static class CountryService
    {
        public static readonly Dictionary<string, string> CountryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Format: [User Input] = [Standardized Code]
            
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
            { "NL", "NL" }, { "Netherlands", "NL" }, { "Holland", "NL" },
            { "SE", "SE" }, { "Sweden", "SE" },
            { "NO", "NO" }, { "Norway", "NO" },
            { "FI", "FI" }, { "Finland", "FI" },
            { "DK", "DK" }, { "Denmark", "DK" },
            { "PT", "PT" }, { "Portugal", "PT" },
            { "CH", "CH" }, { "Switzerland", "CH" },
            { "BE", "BE" }, { "Belgium", "BE" },
            { "AT", "AT" }, { "Austria", "AT" },
            { "IE", "IE" }, { "Ireland", "IE" },
            
            // Asia
            { "JP", "JP" }, { "Japan", "JP" },
            { "IN", "IN" }, { "India", "IN" },
            { "CN", "CN" }, { "China", "CN" },
            { "KR", "KR" }, { "South Korea", "KR" }, { "Korea", "KR" },
            { "SG", "SG" }, { "Singapore", "SG" },
            { "MY", "MY" }, { "Malaysia", "MY" },
            { "TH", "TH" }, { "Thailand", "TH" },
            { "ID", "ID" }, { "Indonesia", "ID" },
            { "PH", "PH" }, { "Philippines", "PH" },
            { "VN", "VN" }, { "Vietnam", "VN" },
            
            // Oceania
            { "AU", "AU" }, { "Australia", "AU" },
            { "NZ", "NZ" }, { "New Zealand", "NZ" },
            
            // Arabian Nations / Middle East
            { "SA", "SA" }, { "Saudi Arabia", "SA" }, { "KSA", "SA" },
            { "AE", "AE" }, { "United Arab Emirates", "AE" }, { "UAE", "AE" }, { "Emirates", "AE" },
            { "QA", "QA" }, { "Qatar", "QA" },
            { "KW", "KW" }, { "Kuwait", "KW" },
            { "BH", "BH" }, { "Bahrain", "BH" },
            { "OM", "OM" }, { "Oman", "OM" },
            { "YE", "YE" }, { "Yemen", "YE" },
            { "IQ", "IQ" }, { "Iraq", "IQ" },
            { "IR", "IR" }, { "Iran", "IR" }, { "Iran Islamic Republic", "IR" },
            { "JO", "JO" }, { "Jordan", "JO" }, { "Hashemite Kingdom of Jordan", "JO" },
            { "LB", "LB" }, { "Lebanon", "LB" },
            { "SY", "SY" }, { "Syria", "SY" }, { "Syrian Arab Republic", "SY" },
            { "PS", "PS" }, { "Palestine", "PS" }, { "Palestinian Territory", "PS" },
            { "EG", "EG" }, { "Egypt", "EG" }, { "Arab Republic of Egypt", "EG" },
            { "LY", "LY" }, { "Libya", "LY" }, { "Libyan Arab Jamahiriya", "LY" },
            { "TN", "TN" }, { "Tunisia", "TN" },
            { "DZ", "DZ" }, { "Algeria", "DZ" }, { "Algerian Republic", "DZ" },
            { "MA", "MA" }, { "Morocco", "MA" }, { "Kingdom of Morocco", "MA" },
            { "SD", "SD" }, { "Sudan", "SD" }, { "Republic of Sudan", "SD" },
            { "SO", "SO" }, { "Somalia", "SO" }, { "Federal Republic of Somalia", "SO" },
            { "DJ", "DJ" }, { "Djibouti", "DJ" }, { "Republic of Djibouti", "DJ" },
            { "MR", "MR" }, { "Mauritania", "MR" }, { "Islamic Republic of Mauritania", "MR" },
            { "KM", "KM" }, { "Comoros", "KM" }, { "Union of Comoros", "KM" }
        };

        public static string NormalizeCountry(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return CountryMap.TryGetValue(input.Trim(), out var normalized)
                ? normalized
                : input.Trim().ToUpper();
        }

        public static List<string> GetCommonCountryNames()
        {
            return CountryMap.Keys
                .Where(k => k.Length > 2) // Prefer full names over codes
                .Distinct()
                .OrderBy(k => k)
                .ToList();
        }
    }
}
