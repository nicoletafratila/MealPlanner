using Blazored.SessionStorage;

namespace Common.Api
{
    public class TokenProvider(ISessionStorageService sessionStorage)
    {
        private readonly ISessionStorageService _sessionStorage =
            sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));

        private const string TokenKey = Constants.MealPlanner.AuthToken;

        public async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _sessionStorage.GetItemAsync<string?>(TokenKey, cancellationToken);
        }

        public async Task SetTokenAsync(string token, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            cancellationToken.ThrowIfCancellationRequested();
            await _sessionStorage.SetItemAsync(TokenKey, token, cancellationToken);
        }

        public async Task RemoveTokenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _sessionStorage.RemoveItemAsync(TokenKey, cancellationToken);
        }
    }
}