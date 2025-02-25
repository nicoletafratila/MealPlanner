using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Common.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "First Name")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "First Name must be alpha characters only.")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Last Name must be alpha characters only.")]
        public string? LastName { get; set; }

        public byte[]? ProfilePicture { get; set; }

        public bool IsActive { get; set; }
    }
}
