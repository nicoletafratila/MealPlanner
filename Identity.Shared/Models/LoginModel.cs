using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace Identity.Shared.Models
{
    /// <summary>
    /// Basic login model used for authentication.
    /// </summary>
    public class LoginModel : BaseModel
    {
        /// <summary>
        /// Username used to log in (required).
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password used to log in.
        /// </summary>
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        /// <summary>
        /// Whether the login should be remembered across sessions.
        /// </summary>
        public bool RememberLogin { get; set; } = false;
    }
}