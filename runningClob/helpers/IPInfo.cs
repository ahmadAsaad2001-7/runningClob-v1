using Newtonsoft.Json;

namespace runningClob.helpers
{
    public class IPInfo
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("loc")]
        public string Loc { get; set; } // Latitude,Longitude

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        // Helper properties
        public string Latitude => Loc?.Split(',')?[0] ?? "0";
        public string Longitude => Loc?.Split(',')?[1] ?? "0";
    }
}