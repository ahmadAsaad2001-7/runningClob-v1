using runningClob.Models;

namespace runningClob.ViewModels;
public class HomeViewModel
{
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string IPAddress { get; set; }

    public IEnumerable<Club>? Clubs { get; set; }
    // Helper properties for display
    public bool HasValidCountry => !string.IsNullOrEmpty(Country) && Country != "Unknown" && Country != "Error";
    public string LocationDescription => HasValidCountry ?
        $"Clubs in {Country}" : "Popular Running Clubs";
}