using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models.ViewModels
{
    public class RegisterVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmedPassword { get; set; }

        [Required]
        public string? Name { get; set; }

        [Display(Name = "Street Address")]
        public string? StreetAddress { get; set; }
        public string? City { get; set; }

        public string? State { get; set; }
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }


        public string? Role { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; }
    }
}
