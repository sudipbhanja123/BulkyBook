using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        [Range(1, 1000, ErrorMessage = "Please enter a valid quantity between 1 and 1000.")]
        public int Count { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        [NotMapped]
        public double Price
        {
            get
            {
                if (Product == null) return 0;
                if (Count <= 50)
                {
                    return Product.Price;
                }
                else if (Count <= 100)
                {
                    return Product.Price50;
                }
                else
                {
                    return Product.Price100;
                }
            }
        }

    }
}
