using System.ComponentModel.DataAnnotations;
using Identity.Shared.Resources;

namespace Identity.Shared.Models
{
    public class RefreshTokenModel
    {
        [Required(ErrorMessageResourceName = nameof(IdentitySharedMessages.RefreshTokenRequired), ErrorMessageResourceType = typeof(IdentitySharedMessages))]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
