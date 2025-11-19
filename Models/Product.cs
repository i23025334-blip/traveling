using System.ComponentModel.DataAnnotations;

namespace Travel.Models
{
    public class Product
    {
        [Required(ErrorMessage = "Please enter Id")]
        public int id { get; set; }

        public string? name { get; set; }

        public decimal price { get; set; }

        public int quantity { get; set; }

        public string? unit { get; set; }

        public string? origin { get; set; }


    }
}
