using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Common.Http;

namespace Common.Core
{
    public class TokenProvider(ISessionStorageService sessionStorage, ILocalStorageService localStorage) : ITokenProvider
    {
        private readonly ISessionStorageService _sessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));
        private readonly ILocalStorageService _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));

        private const string TokenKey = Constants.MealPlanner.AuthToken;

        public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var persistedToken = await _localStorage
                .GetItemAsync<string?>(TokenKey, cancellationToken)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(persistedToken))
                return persistedToken;

            return await _sessionStorage
                .GetItemAsync<string?>(TokenKey, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task SetTokenAsync(string token, bool persistent = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            cancellationToken.ThrowIfCancellationRequested();

            if (persistent)
            {
                await _localStorage.SetItemAsync(TokenKey, token, cancellationToken).ConfigureAwait(false);
                await _sessionStorage.RemoveItemAsync(TokenKey, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _sessionStorage.SetItemAsync(TokenKey, token, cancellationToken).ConfigureAwait(false);
                await _localStorage.RemoveItemAsync(TokenKey, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task RemoveTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _sessionStorage
                .RemoveItemAsync(TokenKey, cancellationToken)
                .ConfigureAwait(false);
            await _localStorage
                .RemoveItemAsync(TokenKey, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}