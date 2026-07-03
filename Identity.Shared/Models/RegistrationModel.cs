using System.ComponentModel.DataAnnotations;using Identity.Shared.Resources;

namespace Identity.Shared.Models
{
    public class RegistrationModel : LoginModel
    {
        [Required(ErrorMessageResourceName = nameof(IdentitySharedMessages.ConfirmPasswordRequired), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceName = nameof(IdentitySharedMessages.ConfirmPasswordMismatch), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessageResourceName = nameof(IdentitySharedMessages.FirstNameAlphaOnly), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        public string? FirstName { get; set; }

        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessageResourceName = nameof(IdentitySharedMessages.LastNameAlphaOnly), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        [EmailAddress(ErrorMessageResourceName = nameof(IdentitySharedMessages.EmailAddressInvalid), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        public string EmailAddress { get; set; } = string.Empty;
    }
}
