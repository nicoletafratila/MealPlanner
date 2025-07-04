using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace Identity.Shared.Models
{
    public class LoginModel : BaseModel
    {
        [Required]
        public string? Username { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberLogin { get; set; } = false;
    }
}
