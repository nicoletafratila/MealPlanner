using System.ComponentModel.DataAnnotations;

namespace Identity.API.Pages.Account.Register
{
    public class InputModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^((?!\.)[\w-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$", ErrorMessage = "The Email field is not a valid e-mail address.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "First Name must be alpha characters only.")]
        [StringLength(int.MaxValue, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Last Name must be alpha characters only.")]
        [StringLength(int.MaxValue, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
