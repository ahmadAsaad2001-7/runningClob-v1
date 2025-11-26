using System.ComponentModel.DataAnnotations;

namespace runningClob.ViewModels
{
    public class RegisterVM
    {
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email Address is required")]
        public string EmailAddress { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        [Required]
        public string ConfirmedPassword { get; set; }
    }
}
