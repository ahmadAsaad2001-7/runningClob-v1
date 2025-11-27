using runningClob.Data.Enum;
using System.ComponentModel.DataAnnotations;

namespace runningClob.ViewModels
{
    public class CreateRaceViewModel
    {
        [Required(ErrorMessage = "Race title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Race category is required")]
        public RaceCategory RaceCategory { get; set; }

        public IFormFile? Image { get; set; }

        // Address fields
        [Required(ErrorMessage = "Street address is required")]
        public string Street { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip code is required")]
        public int ZipCode { get; set; }
        public string Country { get; set; }
      
    }
}