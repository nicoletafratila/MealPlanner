using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace Identity.Shared.Models
{
    /// <summary>
    /// Editable model for managing application user profile and status.
    /// </summary>
    public class ApplicationUserEditModel : BaseModel
    {
        /// <summary>
        /// Unique user identifier (e.g., GUID or external provider id).
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Login/user name (required).
        /// </summary>
        [Required]
        [Display(Name = "User Name")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// First name (letters and spaces only).
        /// </summary>
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "First Name must be alpha characters only.")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name (letters and spaces only).
        /// </summary>
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Last Name must be alpha characters only.")]
        public string? LastName { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public byte[]? ProfilePicture { get; set; }

        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Email address (required, must be valid format).
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Email address is not valid.")]
        public string EmailAddress { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}