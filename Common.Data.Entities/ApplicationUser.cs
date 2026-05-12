using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Common.Data.Entities
{
    public sealed class ApplicationUser : IdentityUser
    {
        [Display(Name = nameof(FirstName))]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "First Name must be alpha characters only.")]
        public string? FirstName { get; set; }

        [Display(Name = nameof(LastName))]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Last Name must be alpha characters only.")]
        public string? LastName { get; set; }

        public byte[]? ProfilePicture { get; set; }

        public bool IsActive { get; set; }

        public string FullName =>
            string.Join(" ", new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}