using System.Text.Json.Serialization;

namespace Common.Models
{
    public class CommandResponse
    {
        [JsonIgnore]
        public bool IsSuccess => Succeeded;

        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }

        public CommandResponse()
        {
        }

        public CommandResponse(bool succeeded, string? message = null, string? errorCode = null)
        {
            Succeeded = succeeded;
            Message = message;
            ErrorCode = errorCode;
        }

        public static CommandResponse Success(string? message = null)
            => new CommandResponse(true, message);

        public static CommandResponse Failed(string error, string? errorCode = null)
            => new CommandResponse(false, error, errorCode);
    }
}
