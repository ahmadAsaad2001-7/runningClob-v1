using Microsoft.AspNetCore.Mvc;
using runningClob.interfaces;
using runningClob.Models;
using runningClob.Services;
using runningClob.ViewModels;
using System.Diagnostics;
using System.Globalization;

namespace runningClob.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClubRepository _clubRepository;
        private readonly IGeolocationService _geolocationService;

        public HomeController(
            ILogger<HomeController> logger,
            IClubRepository clubRepository,
            IGeolocationService geolocationService)
        {
            _logger = logger;
            _clubRepository = clubRepository;
            _geolocationService = geolocationService;
        }

        public async Task<IActionResult> Index()
        {

            var homeViewModel = new HomeViewModel();

            try
            {
                // Get location from API
                var ipInfo = await _geolocationService.GetLocationByIPAsync();

                homeViewModel.City = ipInfo.City;
                homeViewModel.State = ipInfo.Region;
                homeViewModel.Country = ipInfo.Country;
                homeViewModel.IPAddress = ipInfo.Ip;

                if (!string.IsNullOrEmpty(homeViewModel.Country) &&
                    homeViewModel.Country != "Unknown" &&
                    homeViewModel.Country != "Error")
                {
                    // 1. Priority: Clubs from user's country
                    homeViewModel.Clubs = await _clubRepository.GetClubsByCountryAsync(homeViewModel.Country);
                    _logger.LogInformation("Found {Count} clubs in {Country}",
                        homeViewModel.Clubs.Count(), homeViewModel.Country);
                }
                else
                {
                    // 2. Fallback: Show all clubs if country detection fails
                    homeViewModel.Clubs = (List<Club>)await _clubRepository.GetAllAsync();
                    _logger.LogInformation("Country detection failed, showing all clubs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index");
                homeViewModel.Clubs = (List<Club>)await _clubRepository.GetAllAsync();
                homeViewModel.Country = "Error";
            }

            return View(homeViewModel);
        }
        // Action to manually trigger location lookup (for testing)
        [HttpPost]
        public async Task<IActionResult> RefreshLocation()
        {
            try
            {
                var ipInfo = await _geolocationService.GetLocationByIPAsync();
                TempData["LocationMessage"] = $"Location updated: {ipInfo.City}, {ipInfo.Region}";
            }
            catch (Exception ex)
            {
                TempData["LocationError"] = $"Failed to update location: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // Action to test with specific IP address
        [HttpPost]
        public async Task<IActionResult> TestIPLocation(string testIP)
        {
            try
            {
                if (!string.IsNullOrEmpty(testIP))
                {
                    var ipInfo = await _geolocationService.GetLocationByIPAsync(testIP);
                    TempData["TestLocation"] = $"Test IP {testIP}: {ipInfo.City}, {ipInfo.Region}, {ipInfo.Country}";
                }
            }
            catch (Exception ex)
            {
                TempData["TestLocationError"] = $"Test failed: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}