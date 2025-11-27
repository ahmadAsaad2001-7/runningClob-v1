using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using runningClob.Data;
using runningClob.interfaces;
using runningClob.Models;
using runningClob.Services;
using runningClob.ViewModels;
using System.Diagnostics.Metrics;
using System.Security.Claims;

public class ClobController : Controller
{
    private readonly AppDbContext _context;
    private readonly IClubRepository _clubRepository;
    private readonly IPhotoService _photoService;
    private readonly ILogger<ClobController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGeolocationService _geolocationService;
    private readonly ICountryAliasService _countryAliasService;
    public ClobController(
        ICountryAliasService countryAliasService,
        AppDbContext context,
        IClubRepository clubRepository,
        IPhotoService photoService,
        ILogger<ClobController> logger,
        IHttpContextAccessor httpContextAccessor,
        IGeolocationService geolocationService) // Add geolocation service

    {
        _context = context;
        _clubRepository = clubRepository;
        _photoService = photoService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _geolocationService = geolocationService;
        _countryAliasService = countryAliasService;

        _logger.LogInformation("ClubController initialized");
    }

    public async Task<IActionResult> Index()
    {
        
        _logger.LogInformation("ClubController.Index action started");
        var model = new HomeViewModel();

        try
        {
            var clubs = await _context.Clubs.ToListAsync();
            _logger.LogInformation("Retrieved {ClubCount} clubs from database", clubs.Count);

            // Log user location for analytics
            try
            {
                var location = await _geolocationService.GetLocationByIPAsync();
                _logger.LogInformation("User accessing clubs from: {Country}, {City}", location.Country, location.City);
            }
            catch (Exception locEx)
            {
                _logger.LogWarning(locEx, "Failed to get user location for analytics");
            }

            return View(clubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in ClubController.Index");
            throw;
        }
    }
 
    // Updated ClubController Create method
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation("GET Create - User: {UserName}", User.Identity.Name);

        var clubVM = new CreateClubViewModel();

        try
        {
            // Get location from IP
            var ipInfo = await _geolocationService.GetLocationByIPAsync();

            if (ipInfo != null && ipInfo.City != "Unknown")
            {
                var countryCode = ipInfo.Country; // IPInfo returns 2-letter code

                // Convert country code to friendly name for display
                clubVM.Country = _countryAliasService.GetFriendlyCountryName(countryCode);
                clubVM.City = ipInfo.City;
                clubVM.State = ipInfo.Region;

                _logger.LogInformation("📍 Auto-populated location: {City}, {State}, {Country}",
                    ipInfo.City, ipInfo.Region, clubVM.Country);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location from IP");
        }

        return View(clubVM);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClubViewModel clubVM)
    {
        _logger.LogInformation("=== CREATE CLUB STARTED ===");

        // 🎯 DETAILED VALIDATION LOGGING
        if (!ModelState.IsValid)
        {
            _logger.LogError(" MODEL VALIDATION FAILED - DETAILED ERRORS:");

            foreach (var key in ModelState.Keys)
            {
                var entry = ModelState[key];
                foreach (var error in entry.Errors)
                {
                    _logger.LogError("   FIELD: {Field} - ERROR: {Error}", key, error.ErrorMessage);
                }
            }

            _logger.LogInformation("📝 ACTUAL FIELD VALUES RECEIVED:");
            _logger.LogInformation("   Title: {Title}", clubVM.Title ?? "NULL");
            _logger.LogInformation("   Description: {Description}", clubVM.Description ?? "NULL");
            _logger.LogInformation("   ClubCategory: {ClubCategory}", clubVM.ClubCategory);
            _logger.LogInformation("   Image: {ImageExists}", clubVM.Image != null ? "Yes" : "No");
            _logger.LogInformation("   City: {City}", clubVM.City ?? "NULL");
            _logger.LogInformation("   State: {State}", clubVM.State ?? "NULL");
            _logger.LogInformation("   Country: {Country}", clubVM.Country ?? "NULL");
            _logger.LogInformation("   ZipCode: {ZipCode}", clubVM.ZipCode);

            return View(clubVM);
        }

            try
            {
            var normalizedCountry = _countryAliasService.NormalizeCountry(clubVM.Country);

            // Your existing creation logic here...
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(currentUserId))
                {
                    ModelState.AddModelError("", "You must be logged in to create a club");
                    return View(clubVM);
                }

                string imageUrl = null;

                if (clubVM.Image != null && clubVM.Image.Length > 0)
                {
                    var result = await _photoService.AddPhotoAsync(clubVM.Image);
                    if (result.Error != null)
                    {
                        ModelState.AddModelError("Image", "Photo upload failed: " + result.Error.Message);
                        return View(clubVM);
                    }
                    imageUrl = result.Url.ToString();
                }
                else
                {
                    ModelState.AddModelError("Image", "Please select an image");
                    return View(clubVM);
                }

                var club = new Club
                {
                    Title = clubVM.Title,
                    Description = clubVM.Description,
                    Image = imageUrl,
                    ClubCategory = clubVM.ClubCategory,
                    AppUserId = currentUserId,
                    Address = new Address
                    {
                        Street = clubVM.Street,
                        City = clubVM.City,
                        State = clubVM.State,
                        Country = normalizedCountry,
                        ZipCode = clubVM.ZipCode
                    }
                };

                _context.Clubs.Add(club);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ CLUB CREATED SUCCESSFULLY - ID: {ClubId}", club.Id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR CREATING CLUB");
                ModelState.AddModelError("", "An error occurred while creating the club: " + ex.Message);
                return View(clubVM);
            }
        }

 
    [HttpPost]
    public async Task<IActionResult> Edit(int id, EditClubViewModel clubVM)
    {
        _logger.LogInformation("ClubController.Edit POST action started for club ID: {ClubId}", id);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Edit club model validation failed for ID: {ClubId}. Errors: {ModelErrors}",
                id, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            ModelState.AddModelError("", "Failed to edit club");
            return View("Edit", clubVM);
        }

        try
        {
            var userClub = await _clubRepository.GetByIdAsync(id, trackEntity: true);

            if (userClub == null)
            {
                _logger.LogWarning("Club not found for edit, ID: {ClubId}", id);
                return View("Error");
            }

            // Handle image upload if a new image was provided
            if (clubVM.NewImage != null)
            {
                _logger.LogInformation("Processing new image upload for club ID: {ClubId}", id);

                var photoResult = await _photoService.AddPhotoAsync(clubVM.NewImage);

                if (photoResult.Error != null)
                {
                    _logger.LogError("Image upload failed for club edit ID: {ClubId}. Error: {Error}",
                        id, photoResult.Error.Message);

                    ModelState.AddModelError("NewImage", "Photo upload failed");
                    return View(clubVM);
                }

                // Delete old image if it exists
                if (!string.IsNullOrEmpty(userClub.Image))
                {
                    _logger.LogInformation("Deleting old image for club ID: {ClubId}", id);
                    _ = _photoService.DeletePhotoAsync(userClub.Image);
                }

                userClub.Image = photoResult.Url.ToString();
                _logger.LogInformation("New image set for club ID: {ClubId}", id);
            }

            // Log location changes
            _logger.LogInformation("Updating club location from '{OldCity}, {OldState}' to '{NewCity}, {NewState}'",
                userClub.Address?.City, userClub.Address?.State, clubVM.City, clubVM.State);

            // Update club properties
            userClub.Title = clubVM.Title;
            userClub.Description = clubVM.Description;
            userClub.ClubCategory = clubVM.ClubCategory;

            // Update address
            if (userClub.Address != null)
            {
                userClub.Address.Street = clubVM.Street;
                userClub.Address.City = clubVM.City;
                userClub.Address.State = clubVM.State;
                userClub.Address.ZipCode = clubVM.ZipCode;
            }

            var result = await _clubRepository.UpdateAsync(userClub);

            if (!result)
            {
                _logger.LogError("Failed to update club in database for ID: {ClubId}", id);
                ModelState.AddModelError("", "Failed to update club in database");
                return View(clubVM);
            }

            _logger.LogInformation("Club updated successfully. ID: {ClubId}, Title: {ClubTitle}", id, clubVM.Title);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating club ID: {ClubId}", id);
            ModelState.AddModelError("", "An error occurred while updating the club");
            return View(clubVM);
        }
    }
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        _logger.LogInformation("Viewing club details for ID: {ClubId}", id);

        try
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null)
            {
                _logger.LogWarning("Club not found for detail view, ID: {ClubId}", id);
                return NotFound();
            }

            _logger.LogInformation("Displaying club details: {ClubTitle}", club.Title);
            return View(club);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving club details for ID: {ClubId}", id);
            return View("Error");
        }
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("ClubController.Delete GET action started for club ID: {ClubId}", id);

        try
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null)
            {
                _logger.LogWarning("Club not found for deletion confirmation, ID: {ClubId}", id);
                return NotFound();
            }

            _logger.LogInformation("Delete confirmation view prepared for club: {ClubTitle}", club.Title);
            return View(club);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ClubController.Delete GET for club ID: {ClubId}", id);
            throw;
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation("ClubController.DeleteConfirmed action started for club ID: {ClubId}", id);

        try
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null)
            {
                _logger.LogWarning("Club not found for deletion, ID: {ClubId}", id);
                return NotFound();
            }

            // Log club details before deletion
            _logger.LogInformation("Deleting club: {ClubTitle} from {ClubCity}, {ClubState}",
                club.Title, club.Address?.City, club.Address?.State);

            // Delete image if exists
            if (!string.IsNullOrEmpty(club.Image))
            {
                _logger.LogInformation("Deleting club image for ID: {ClubId}", id);
                await _photoService.DeletePhotoAsync(club.Image);
            }

            await _clubRepository.DeleteAsync(club);

            _logger.LogInformation("Club deleted successfully. ID: {ClubId}, Title: {ClubTitle}", id, club.Title);

            TempData["SuccessMessage"] = "Club deleted successfully";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting club with ID: {ClubId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the club";
            return RedirectToAction("Index");
        }
    }
    [HttpGet]
    public async Task<IActionResult> FilterClubsByCountry(string country)
    {
        _logger.LogInformation("🌐 AJAX FILTER: Request for country: '{Country}'", country);

        try
        {
            var countryClubs = await _clubRepository.GetClubsByCountryAsync(country);

            _logger.LogInformation("🌐 AJAX FILTER: Repository returned {Count} clubs", countryClubs?.Count() ?? 0);

            if (countryClubs == null || !countryClubs.Any())
            {
                _logger.LogWarning("🌐 AJAX FILTER: No clubs found for country '{Country}'", country);

                return Content(@"
                <div class='col-12 text-center'>
                    <div class='alert alert-warning'>
                        <i class='fas fa-info-circle'></i> No clubs found in <strong>" + country + @"</strong>.
                        <br><small>Try a different country or <a href='javascript:location.reload()'>show all clubs</a></small>
                    </div>
                </div>");
            }

            _logger.LogInformation("🌐 AJAX FILTER: Successfully found {Count} clubs for country '{Country}'",
                countryClubs.Count(), country);

            // Return partial HTML with filtered clubs
            return PartialView("_ClubListPartial", countryClubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ AJAX FILTER: Error for country: {Country}", country);
            return Content(@"
            <div class='col-12 text-center'>
                <div class='alert alert-danger'>
                    <i class='fas fa-exclamation-triangle'></i> Error loading clubs. Please try again.
                </div>
            </div>");
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetUserCountry()
    {
        try
        {
            var ipInfo = await _geolocationService.GetLocationByIPAsync();
            if (ipInfo != null && !string.IsNullOrEmpty(ipInfo.Country))
            {
                // Convert country code to friendly name
                var friendlyName = _countryAliasService.GetFriendlyCountryName(ipInfo.Country);

                return Json(new
                {
                    success = true,
                    country = ipInfo.Country,
                    countryName = friendlyName
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user country for AJAX request");
        }

        return Json(new { success = false });
    }
    [HttpGet]
    public async Task<IActionResult> GetClubsByCountryAsync(string country)
    {
        _logger.LogInformation("🎯 SIMPLE FILTER: Searching for clubs in: '{Country}'", country);

        // If no country specified, show empty page with form
        if (string.IsNullOrWhiteSpace(country))
        {
            _logger.LogInformation("🎯 SIMPLE FILTER: No country specified, showing filter form");
            return View(new List<Club>());
        }

        try
        {
            _logger.LogInformation("🎯 SIMPLE FILTER: Calling repository with: '{Country}'", country);

            // Use the repository method
            var clubs = await _clubRepository.GetClubsByCountryAsync(country);

            _logger.LogInformation("🎯 SIMPLE FILTER: Repository returned {Count} clubs", clubs?.Count() ?? 0);

            if (clubs == null || !clubs.Any())
            {
                _logger.LogWarning("🎯 SIMPLE FILTER: No clubs found for '{Country}'", country);
                ViewBag.Message = $"No clubs found in {country}";
                return View(new List<Club>());
            }

            _logger.LogInformation("🎯 SIMPLE FILTER: Success! Found {Count} clubs in '{Country}'", clubs.Count(), country);
            ViewBag.Message = $"Found {clubs.Count()} clubs in {country}";
            return View(clubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SIMPLE FILTER: Error searching for clubs in '{Country}'", country);
            ViewBag.Message = "An error occurred while searching for clubs";
            return View(new List<Club>());
        }
    }
}