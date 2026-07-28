namespace Common.Http
{
    public interface ITokenProvider
    {
        Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
        Task SetTokenAsync(string token, bool persistent = false, CancellationToken cancellationToken = default);
        Task RemoveTokenAsync(CancellationToken cancellationToken = default);

        Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default);
        Task SetRefreshTokenAsync(string refreshToken, bool persistent = false, CancellationToken cancellationToken = default);
        Task RemoveRefreshTokenAsync(CancellationToken cancellationToken = default);
    }
}
