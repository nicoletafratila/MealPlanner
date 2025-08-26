namespace Common.Models
{
    public class LoginCommandResponse : CommandResponse
    {
        public string? Username { get; set; }
        public string? JwtBearer { get; set; }

        public LoginCommandResponse()
        {
        }

        public LoginCommandResponse(bool succeeded, string message = "", string errorCode = "")
            : base(succeeded, message, errorCode)
        {
        }

        public LoginCommandResponse(bool succeeded, string message = "", string errorCode = "", string token = "")
            : base(succeeded, message, errorCode)
        {
            JwtBearer = token;
        }
    }
}
