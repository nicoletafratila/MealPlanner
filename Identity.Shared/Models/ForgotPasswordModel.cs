using System.ComponentModel.DataAnnotations;
using Common.Constants;

namespace Identity.Shared.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        public InputSource? Source { get; set; }
    }
}
