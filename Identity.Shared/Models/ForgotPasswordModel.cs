using System.ComponentModel.DataAnnotations;

namespace Identity.Shared.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
    }
}
