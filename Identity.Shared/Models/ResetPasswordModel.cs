using System.ComponentModel.DataAnnotations;
using Identity.Shared.Resources;

namespace Identity.Shared.Models
{
    public class ResetPasswordModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = nameof(IdentitySharedMessages.NewPasswordRequired), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = nameof(IdentitySharedMessages.ConfirmPasswordRequired), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
