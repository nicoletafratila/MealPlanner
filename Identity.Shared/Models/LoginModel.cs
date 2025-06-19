using System.ComponentModel.DataAnnotations;

namespace Identity.Shared.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberLogin { get; set; } = false;
    }
}
