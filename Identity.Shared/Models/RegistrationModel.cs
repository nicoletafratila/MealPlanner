using System.ComponentModel.DataAnnotations;
using Identity.Shared.Resources;

namespace Identity.Shared.Models
{
    public class RegistrationModel : LoginModel
    {
        [Required(ErrorMessageResourceName = nameof(IdentitySharedMessages.ConfirmPasswordRequired), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceName = nameof(IdentitySharedMessages.ConfirmPasswordMismatch), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
