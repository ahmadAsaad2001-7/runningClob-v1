using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using runningClob.Data;
using runningClob.Models;
using runningClob.ViewModels;

namespace runningClob.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context,UserManager<AppUser> userManager , SignInManager<AppUser> signInManager)
        { 
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public IActionResult Login()
        {
            var response = new LoginVM();

            return View(response);
        }
        [HttpPost]
        public IActionResult Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);
            var user = _userManager.FindByEmailAsync(loginVM.EmailAddress).Result;
            if (user != null)
            {
                var signInResult = _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false).Result;
                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index", "Race");
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                return View(loginVM);
            }
            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            return View(loginVM);
        }
        public IActionResult Register()
        {
            var response = new RegisterVM();

            return View(response);
        }
        [HttpPost]
        public IActionResult Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);
            var u = _userManager.FindByEmailAsync(registerVM.EmailAddress).Result;
            if (u != null)
            {
                ModelState.AddModelError(string.Empty, "This email address is already in use.");
                return View(registerVM);
            }


            var user = new AppUser
            {
                UserName = registerVM.EmailAddress,
                Email = registerVM.EmailAddress
            };

            var result = _userManager.CreateAsync(user, registerVM.Password).Result;

            if (result.Succeeded)
            {
                _signInManager.SignInAsync(user, false).Wait();
                return RedirectToAction("Index");

            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(registerVM);
        }
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction("Index", "Race");
        }
    }
}


