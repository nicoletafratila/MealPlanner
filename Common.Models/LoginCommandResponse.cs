namespace Common.Models
{
    public class LoginCommandResponse : CommandResponse
    {
        public string? Username { get; set; }

        public string? JwtBearer { get; set; }

        public IList<KeyValuePair<string, string>> Claims { get; set; } = [];

        /// <summary>
        /// Convenience flag indicating a successful, token-bearing login.
        /// </summary>
        public bool IsAuthenticated => Succeeded && !string.IsNullOrWhiteSpace(JwtBearer);

        public LoginCommandResponse()
        {
        }

        public LoginCommandResponse(bool succeeded, string message = "", string errorCode = "")
            : base(succeeded, message, errorCode)
        {
        }

        public LoginCommandResponse(
            bool succeeded,
            string message = "",
            string errorCode = "",
            string token = "",
            string? username = null,
            IEnumerable<KeyValuePair<string, string>>? claims = null)
            : base(succeeded, message, errorCode)
        {
            JwtBearer = token;
            Username = username;

            if (claims is not null)
            {
                foreach (var kv in claims)
                {
                    Claims.Add(kv);
                }
            }
        }
    }
}