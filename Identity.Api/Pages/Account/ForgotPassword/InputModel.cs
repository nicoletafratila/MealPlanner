using System.ComponentModel.DataAnnotations;

namespace Identity.API.Pages.Account.ForgotPassword
{
    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
