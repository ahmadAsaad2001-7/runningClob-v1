using Microsoft.AspNetCore.Mvc;
using runningClob.interfaces;
using runningClob.ViewModels;

namespace runningClob.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userrepository;
        public UserController(IUserRepository userRepository)
        {
            _userrepository = userRepository;  
        }
        [HttpGet("users")]
        public async Task<IActionResult> Index()
        {
            var users = await _userrepository.GetAllUsers();
            List<UserVm> userVms = new List<UserVm>();
            foreach (var user in users)
            {
                var userVm = new UserVm
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    pace = user.Pace,
                    Mileage = user.Mileage
                };
                userVms.Add(userVm);
            }
           
            return View(userVms);
        }
        [HttpGet("User/Detail/{id}")]  // Add this alias
        [HttpGet("User/Details/{id}")] // Keep the original
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userrepository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            var userVm = new UserVm
            {
                Id = user.Id,
                UserName = user.UserName,
                pace = user.Pace,
                Mileage = user.Mileage,
                ProfileImageUrl= user.ProfileImageUrl
            };
            return View(userVm);
        }
   
        public async Task<IActionResult> Edit()
        {

            return View();
        }


    }
}
