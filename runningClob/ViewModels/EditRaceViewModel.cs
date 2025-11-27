using runningClob.Data.Enum;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using runningClob.Data.Enum;
using runningClob.Models;
using System.ComponentModel.DataAnnotations;

namespace runningClob.ViewModels
{
    public class EditRaceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Club name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public RaceCategory RaceCategory { get; set; }

        // For displaying current image
        public string Image { get; set; }

        // For file upload - make this nullable so it's not required
        [Display(Name = "New Image")]
        public IFormFile? NewImage { get; set; }
        
       public  string Country { get; set; }
        // Address fields
        [Required(ErrorMessage = "Street address is required")]
        public string Street { get; set; }

        [Required(ErrorMessage = "ZipCode is required")]
        public int ZipCode { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        // Hidden field for address ID
        public int? AddressId { get; set; }
    }
}