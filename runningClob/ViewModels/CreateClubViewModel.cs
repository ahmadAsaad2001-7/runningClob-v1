using runningClob.Data.Enum;
using System.ComponentModel.DataAnnotations;
namespace runningClob.ViewModels
{

    public class CreateClubViewModel
    {
        [Required(ErrorMessage = "Club name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public ClubCategory ClubCategory { get; set; }

        [Required(ErrorMessage = "Image is required")]
        public IFormFile Image { get; set; }

        // Address fields
        public string Street { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Zip code is required")]
        public int ZipCode { get; set; }

    }
}