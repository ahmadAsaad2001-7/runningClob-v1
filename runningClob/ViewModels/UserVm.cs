using runningClob.Models;

namespace runningClob.ViewModels
{
    public class UserVm
    {
        public string Id { get; set; }  
        public string UserName { get; set; }
        public int? pace { get; set; }
        public int? Mileage { get; set; }
        public string ProfileImageUrl { get; set; }

       
    }
}
