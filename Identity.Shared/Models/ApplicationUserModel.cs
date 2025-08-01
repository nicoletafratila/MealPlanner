using System.ComponentModel.DataAnnotations;

namespace Identity.Shared.Models
{
    public class ApplicationUserModel
    {
        public string? UserId { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string? Username { get; set; }

        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "First Name must be alpha characters only.")]
        public string? FirstName { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Last Name must be alpha characters only.")]
        public string? LastName { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public byte[]? ProfilePicture { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email address is not valid.")]
        public string? EmailAddress { get; set; }

        public bool IsActive { get; set; }
    }
}
