using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using runningClob.Data;
using runningClob.interfaces;
using runningClob.Models;
using runningClob.repository.RaceRepositroy;
using runningClob.Services;
using runningClob.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace runningClob.Controllers
{
    public class RaceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRaceRepository _raceRepository;
        private readonly IPhotoService _photoService;
        private readonly ILogger<RaceController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGeolocationService _geolocationService;
        private readonly ICountryAliasService _countryAliasService;
        public RaceController(AppDbContext context, IRaceRepository raceRepository, IPhotoService photoService,ICountryAliasService countryAliasService , ILogger<RaceController> logger,IHttpContextAccessor httpContextAccessor,IGeolocationService geolocationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _raceRepository = raceRepository;
            _photoService = photoService;
            _logger = logger;
            _geolocationService = geolocationService;
            _countryAliasService = countryAliasService;
        }

        public async Task<IActionResult> Index()
        {
            List<Race> races = await _context.Races.Include(r => r.Address).ToListAsync();
            return View(races);
        }

        // GET: Add this action to display the form
        // Updated RaceController Create method
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("GET Create - User: {UserName}", User.Identity.Name);

            var raceVM = new CreateRaceViewModel();

            try
            {
                // Get location from IP
                var ipInfo = await _geolocationService.GetLocationByIPAsync();

                if (ipInfo != null && ipInfo.City != "Unknown")
                {
                    var countryCode = ipInfo.Country; // IPInfo returns 2-letter code

                    // Convert country code to friendly name for display
                    raceVM.Country = _countryAliasService.GetFriendlyCountryName(countryCode);
                    raceVM.City = ipInfo.City;
                    raceVM.State = ipInfo.Region;

                    _logger.LogInformation("📍 Auto-populated location: {City}, {State}, {Country}",
                        ipInfo.City, ipInfo.Region, raceVM.Country);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location from IP");
            }

            return View(raceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRaceViewModel raceVM)
        {

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("=== CREATE Race STARTED ===");

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
                _logger.LogInformation("   Title: {Title}", raceVM.Title ?? "NULL");
                _logger.LogInformation("   Description: {Description}", raceVM.Description ?? "NULL");
                _logger.LogInformation("   RaceCategory: {RaceCategory}", raceVM.RaceCategory);
                _logger.LogInformation("   Image: {ImageExists}", raceVM.Image != null ? "Yes" : "No");
                _logger.LogInformation("   City: {City}", raceVM.City ?? "NULL");
                _logger.LogInformation("   State: {State}", raceVM.State ?? "NULL");
                _logger.LogInformation("   Country: {Country}", raceVM.Country ?? "NULL");
                _logger.LogInformation("   ZipCode: {ZipCode}", raceVM.ZipCode);

                return View(raceVM);
            }

            try
            {
                var normalizedCountry = _countryAliasService.NormalizeCountry(raceVM.Country);

                // Your existing creation logic here...
                

                if (string.IsNullOrEmpty(currentUserId))
                {
                    ModelState.AddModelError("", "You must be logged in to create a Race");
                    return View(raceVM);
                }

                string imageUrl = null;

                if (raceVM.Image != null && raceVM.Image.Length > 0)
                {
                    var result = await _photoService.AddPhotoAsync(raceVM.Image);
                    if (result.Error != null)
                    {
                        ModelState.AddModelError("Image", "Photo upload failed: " + result.Error.Message);
                        return View(raceVM);
                    }
                    imageUrl = result.Url.ToString();
                }
                else
                {
                    ModelState.AddModelError("Image", "Please select an image");
                    return View(raceVM);
                }

                var race = new Race
                {
                    Title = raceVM.Title,
                    Description = raceVM.Description,
                    Image = imageUrl,
                    RaceCategory = raceVM.RaceCategory,
                    AppUserId = currentUserId,
                    Address = new Address
                    {
                        Street = raceVM.Street,
                        City = raceVM.City,
                        State = raceVM.State,
                        Country = normalizedCountry,
                        ZipCode = raceVM.ZipCode
                    }
                };

                _context.Races.Add(race);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Race CREATED SUCCESSFULLY - ID: {RaceId}", race.Id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR CREATING Race");
                ModelState.AddModelError("", "An error occurred while creating the Race: " + ex.Message);
                return View(raceVM);
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel raceVM)
        {
            _logger.LogInformation("RaceController.Edit POST action started for race ID: {RaceId}", id);
            _logger.LogInformation("Editing race: {RaceTitle}", raceVM.Title);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Edit race model validation failed for ID: {RaceId}. Errors: {ModelErrors}",
                    id, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                ModelState.AddModelError("", "Failed to edit race");
                return View("Edit", raceVM);
            }

            try
            {
                var userRace = await _raceRepository.GetByIdAsync(id, trackEntity: true);

                if (userRace == null)
                {
                    _logger.LogWarning("Race not found for edit, ID: {RaceId}", id);
                    return View("Error");
                }

                // Log the current user for audit purposes
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("User {UserId} editing race ID: {RaceId}", currentUserId, id);

                // Handle image upload if a new image was provided
                if (raceVM.NewImage != null && raceVM.NewImage.Length > 0)
                {
                    _logger.LogInformation("Processing new image upload for race ID: {RaceId}", id);

                    var photoResult = await _photoService.AddPhotoAsync(raceVM.NewImage);

                    if (photoResult.Error != null)
                    {
                        _logger.LogError("Image upload failed for race edit ID: {RaceId}. Error: {Error}",
                            id, photoResult.Error.Message);

                        ModelState.AddModelError("NewImage", "Photo upload failed: " + photoResult.Error.Message);
                        return View(raceVM);
                    }

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(userRace.Image))
                    {
                        _logger.LogInformation("Deleting old image for race ID: {RaceId}", id);
                        try
                        {
                            await _photoService.DeletePhotoAsync(userRace.Image);
                            _logger.LogInformation("Old image deleted successfully for race ID: {RaceId}", id);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, "Failed to delete old image for race ID: {RaceId}", id);
                            // Continue with update even if old image deletion fails
                        }
                    }

                    userRace.Image = photoResult.Url.ToString();
                    _logger.LogInformation("New image set for race ID: {RaceId}. URL: {ImageUrl}", id, userRace.Image);
                }
                else
                {
                    _logger.LogInformation("No new image provided for race ID: {RaceId}, keeping existing image", id);
                }

                // Log location changes for analytics
                _logger.LogInformation("Updating race location from '{OldCity}, {OldState}, {OldCountry}' to '{NewCity}, {NewState}, {NewCountry}'",
                    userRace.Address?.City, userRace.Address?.State, userRace.Address?.Country,
                    raceVM.City, raceVM.State, raceVM.Country);

                // Update race properties
                userRace.Title = raceVM.Title;
                userRace.Description = raceVM.Description;
                userRace.RaceCategory = raceVM.RaceCategory; // Fixed casing: raceCategory -> RaceCategory

                _logger.LogInformation("Updated race properties - Title: {Title}, Category: {Category}",
                    userRace.Title, userRace.RaceCategory);

                // Update address - INCLUDING COUNTRY
                if (userRace.Address != null)
                {
                    userRace.Address.Street = raceVM.Street;
                    userRace.Address.City = raceVM.City;
                    userRace.Address.State = raceVM.State;
                    userRace.Address.Country = raceVM.Country;
                    userRace.Address.ZipCode = raceVM.ZipCode;

                    _logger.LogInformation("Address updated for race ID: {RaceId}", id);
                }
                else
                {
                    _logger.LogWarning("No address found for race ID: {RaceId}, creating new address", id);
                    userRace.Address = new Address
                    {
                        Street = raceVM.Street,
                        City = raceVM.City,
                        State = raceVM.State,
                        Country = raceVM.Country, 
                        ZipCode = raceVM.ZipCode
                    };
                }

                // Log geolocation context
                try
                {
                    var userLocation = await _geolocationService.GetLocationByIPAsync();
                    _logger.LogInformation("User editing race from: {UserCity}, {UserCountry}",
                        userLocation.City, userLocation.Country);
                }
                catch (Exception locEx)
                {
                    _logger.LogDebug(locEx, "Could not get user location during race edit");
                }

                var result = await _raceRepository.UpdateAsync(userRace);

                if (!result)
                {
                    _logger.LogError("Failed to update race in database for ID: {RaceId}", id);
                    ModelState.AddModelError("", "Failed to update race in database");
                    return View(raceVM);
                }

                _logger.LogInformation("Race updated successfully. ID: {RaceId}, Title: {RaceTitle}", id, raceVM.Title);

                // Add success message
                TempData["SuccessMessage"] = $"Race '{raceVM.Title}' updated successfully!";

                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error for race ID: {RaceId}", id);
                ModelState.AddModelError("", "A database error occurred while updating the race. Please try again.");
                return View(raceVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating race ID: {RaceId}", id);
                ModelState.AddModelError("", "An unexpected error occurred while updating the race: " + ex.Message);
                return View(raceVM);
            }
            finally
            {
                _logger.LogInformation("RaceController.Edit POST action completed for race ID: {RaceId}", id);
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null)
            {
                return NotFound();
            }

            return View(race); // Show confirmation view
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var race = await _raceRepository.GetByIdAsync(id);
                if (race == null)
                {
                    return NotFound();
                }

                // Delete image if exists
                if (!string.IsNullOrEmpty(race.Image))
                {
                    await _photoService.DeletePhotoAsync(race.Image);
                }

                await _raceRepository.DeleteAsync(race);

                // Add success message
                TempData["SuccessMessage"] = "race deleted successfully";

                // Make sure to redirect to the Index ACTION, not just the view
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error deleting race with id: {raceId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the race";
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            _logger.LogInformation("Viewing race details for ID: {RaceId}", id);

            try
            {
                var race = await _raceRepository.GetByIdAsync(id);
                if (race == null)
                {
                    _logger.LogWarning("Race not found for detail view, ID: {RaceId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Displaying race details: {raceTitle}", race.Title);
                return View(race);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving race details for ID: {RaceId}", id);
                return View("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> FilterRacesByCountry(string country)
        {
            _logger.LogInformation("🌐 AJAX RACE FILTER: Request for country: '{Country}'", country);

            try
            {
                var countryRaces = await _raceRepository.GetRacesByCountryAsync(country);

                _logger.LogInformation("🌐 AJAX RACE FILTER: Repository returned {Count} races", countryRaces?.Count() ?? 0);

                if (countryRaces == null || !countryRaces.Any())
                {
                    _logger.LogWarning("🌐 AJAX RACE FILTER: No races found for country '{Country}'", country);

                    return Content(@"
            <div class='col-12 text-center'>
                <div class='alert alert-warning'>
                    <i class='fas fa-info-circle'></i> No races found in <strong>" + country + @"</strong>.
                    <br><small>Try a different country or <a href='javascript:location.reload()'>show all races</a></small>
                </div>
            </div>");
                }

                _logger.LogInformation("🌐 AJAX RACE FILTER: Successfully found {Count} races for country '{Country}'",
                    countryRaces.Count(), country);

                // Return partial HTML with filtered races
                return PartialView("_RaceListPartial", countryRaces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AJAX RACE FILTER: Error for country: {Country}", country);
                return Content(@"
        <div class='col-12 text-center'>
            <div class='alert alert-danger'>
                <i class='fas fa-exclamation-triangle'></i> Error loading races. Please try again.
            </div>
        </div>");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRacesByCountryAsync(string country)
        {
            _logger.LogInformation("🎯 SIMPLE RACE FILTER: Searching for races in: '{Country}'", country);

            // If no country specified, show empty page with form
            if (string.IsNullOrWhiteSpace(country))
            {
                _logger.LogInformation("🎯 SIMPLE RACE FILTER: No country specified, showing filter form");
                return View(new List<Race>());
            }

            try
            {
                _logger.LogInformation("🎯 SIMPLE RACE FILTER: Calling repository with: '{Country}'", country);

                // Use the repository method
                var races = await _raceRepository.GetRacesByCountryAsync(country);

                _logger.LogInformation("🎯 SIMPLE RACE FILTER: Repository returned {Count} races", races?.Count() ?? 0);

                if (races == null || !races.Any())
                {
                    _logger.LogWarning("🎯 SIMPLE RACE FILTER: No races found for '{Country}'", country);
                    ViewBag.Message = $"No races found in {country}";
                    return View(new List<Race>());
                }

                _logger.LogInformation("🎯 SIMPLE RACE FILTER: Success! Found {Count} races in '{Country}'", races.Count(), country);
                ViewBag.Message = $"Found {races.Count()} races in {country}";
                return View(races);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SIMPLE RACE FILTER: Error searching for races in '{Country}'", country);
                ViewBag.Message = "An error occurred while searching for races";
                return View(new List<Race>());
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
    }
}