using System.ComponentModel.DataAnnotations;

namespace runningClob.Models
{
    public class Address

    {
        [Key]
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public int ZipCode { get; set; }

        
    }
}
