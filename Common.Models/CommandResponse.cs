namespace Common.Models
{
    public class CommandResponse
    {
        public bool Succeeded { get; set; } = false;
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }

        public CommandResponse()
        {
        }

        public CommandResponse(bool succeeded, string message = null, string errorCode = null)
        {
            Succeeded = succeeded;
            Message = message;
            ErrorCode = errorCode;
        }

        public static CommandResponse Success()
            => new CommandResponse(true);

        public static CommandResponse Failed(string error)
            => new CommandResponse(false, error);
    }

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
