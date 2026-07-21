using System.ComponentModel.DataAnnotations;
using Identity.Shared.Resources;

namespace Identity.Shared.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessageResourceName = nameof(IdentitySharedMessages.EmailAddressRequired), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        [EmailAddress(ErrorMessageResourceName = nameof(IdentitySharedMessages.EmailAddressInvalid), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        public string EmailAddress { get; set; } = string.Empty;
    }
}
