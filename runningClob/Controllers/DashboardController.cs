using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using runningClob;
using runningClob.interfaces;
using runningClob.Models;
using runningClob.ViewModels;

public class DashboardController : Controller
{
    private readonly IDashboardRepository _dashboardRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPhotoService _photoService;
    public DashboardController(IDashboardRepository dashboardRepo, IHttpContextAccessor httpContextAccessor, IPhotoService photoService)
    {
        _dashboardRepo = dashboardRepo;
        _httpContextAccessor = httpContextAccessor;
        _photoService = photoService;
    }

    public async Task<IActionResult> Index()
    {
        // Get both clubs and races for the current user
        var userClubs = await _dashboardRepo.GetAllUserClubs();
        var userRaces = await _dashboardRepo.GetAllUserRaces();

        // Create the ViewModel
        var dashboardVM = new DashboardVM
        {
            clubs = userClubs,
            races = userRaces
        };

        return View(dashboardVM); // Pass the ViewModel, not just clubs
    }
    public async Task<IActionResult> EditUserProfile()
    {
        var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
        var user = await _dashboardRepo.GetUserById(curUserId);
        if (user == null)
        {
            return NotFound("User not found");
        }
        var editUserVM = new EditUserDashboardVM
        {
            Id = user.Id,
            Pace = user.Pace,
            Mileage = user.Mileage,
            City = user.City,
            State = user.State,
            ProfileImageUrl = user.ProfileImageUrl
        };

        return View(editUserVM); // Pass the model to the view
    }
    private void MapUserEditVMToUser(EditUserDashboardVM editVM, AppUser user, ImageUploadResult photoResult)
    {
        user.Id = editVM.Id;
        user.ProfileImageUrl = photoResult.Url.ToString();
        user.Pace = editVM.Pace;
        user.Mileage = editVM.Mileage;
        user.City = editVM.City;
        user.State = editVM.State;
        // Note: ProfileImageUrl is handled separately
    }
    [HttpPost]
    public async Task<IActionResult> EditUserProfile(EditUserDashboardVM EditVM)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Failed to edit profile");
            return View(EditVM);
        }
        var user = await _dashboardRepo.GetByIdNoTracking(EditVM.Id);
        if (user.ProfileImageUrl == "" || user.ProfileImageUrl == null)
        {
            var photoResult = await _photoService.AddPhotoAsync(EditVM.ProfileImage);
            MapUserEditVMToUser(EditVM, user, photoResult);
            _dashboardRepo.UpdateUser(user);
            return RedirectToAction("Index");
        }
        else
        {
            try
            {
                await _photoService.DeletePhotoAsync(user.ProfileImageUrl);

            }
            catch 
            {
                ModelState.AddModelError("","could not delete photo");
                return View(EditVM);
            }
            var photoResult = await _photoService.AddPhotoAsync(EditVM.ProfileImage);
            MapUserEditVMToUser(EditVM, user, photoResult);
            _dashboardRepo.UpdateUser(user);
            return RedirectToAction("Index");


        }


    } 
}