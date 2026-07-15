using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Identity.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = nameof(FirstName))]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessageResourceName = nameof(Resources.EntityMessages.FirstNameAlphaOnly), ErrorMessageResourceType = typeof(Resources.EntityMessages))]
        public string? FirstName { get; set; }

        [Display(Name = nameof(LastName))]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessageResourceName = nameof(Resources.EntityMessages.LastNameAlphaOnly), ErrorMessageResourceType = typeof(Resources.EntityMessages))]
        public string? LastName { get; set; }

        public byte[]? ProfilePicture { get; set; }

        public bool IsActive { get; set; }

        public string FullName =>
            string.Join(" ", new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
