using Common.Models;

namespace Identity.Shared.Models
{
    public class ApplicationUserModel : BaseModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public bool IsActive { get; set; }
    }
}
