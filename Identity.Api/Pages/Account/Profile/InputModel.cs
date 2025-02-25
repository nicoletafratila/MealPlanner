using System.ComponentModel.DataAnnotations;

namespace Identity.API.Pages.Account.Profile
{
    public class InputModel
    {

        [Phone]
        [RegularExpression(@"^([+]?\d{1,2}[-\s]?|)\d{3}[-\s]?\d{3}[-\s]?\d{4}$", ErrorMessage = "The Phone Number field is not a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [RegularExpression(@"^((?!\.)[\w-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$", ErrorMessage = "The Email field is not a valid e-mail address.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "First Name must be alpha characters only.")]
        [StringLength(int.MaxValue, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Last Name must be alpha characters only.")]
        [StringLength(int.MaxValue, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
