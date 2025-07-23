namespace Common.Models
{
    public class LoginCommandResponse : CommandResponse
    {
        public string? JwtBearer { get; set; }

        public LoginCommandResponse()
        {
        }

        public LoginCommandResponse(bool succeeded, string message = null, string errorCode = null)
            : base(succeeded, message, errorCode)
        {
        }

        public LoginCommandResponse(bool succeeded, string message = null, string errorCode = null, string token = null)
            : base(succeeded, message, errorCode)
        {
            JwtBearer = token;
        }
    }
}
