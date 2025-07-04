namespace Identity.Api.Features.Authentication.Commands.Login
{
    public record struct LoginCommandResponse
    {
        public string? Message { get; set; }
        public string? JwtBearer { get; set; }
        public bool Success { get; set; }
    }
}
