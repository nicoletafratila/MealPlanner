namespace Common.Http
{
    public interface ITokenProvider
    {
        Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
        Task SetTokenAsync(string token, CancellationToken cancellationToken = default);
        Task RemoveTokenAsync(CancellationToken cancellationToken = default);
    }
}
