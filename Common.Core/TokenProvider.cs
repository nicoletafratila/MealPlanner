using Blazored.SessionStorage;
using Common.Http;

namespace Common.Core
{
    public class TokenProvider(ISessionStorageService sessionStorage) : ITokenProvider
    {
        private readonly ISessionStorageService _sessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));

        private const string TokenKey = Constants.MealPlanner.AuthToken;

        public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _sessionStorage
                .GetItemAsync<string?>(TokenKey, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task SetTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            cancellationToken.ThrowIfCancellationRequested();

            await _sessionStorage
                .SetItemAsync(TokenKey, token, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task RemoveTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _sessionStorage
                .RemoveItemAsync(TokenKey, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}