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

        public CommandResponse(bool succeeded, string message = "", string errorCode = "")
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


    public class LoginResponse :CommandResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
    }

}
