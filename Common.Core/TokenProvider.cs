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
        private const string RefreshTokenKey = Constants.MealPlanner.RefreshToken;

        public Task<string?> GetTokenAsync(CancellationToken cancellationToken = default) =>
            GetAsync(TokenKey, cancellationToken);

        public Task SetTokenAsync(string token, bool persistent = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            return SetAsync(TokenKey, token, persistent, cancellationToken);
        }

        public Task RemoveTokenAsync(CancellationToken cancellationToken = default) =>
            RemoveAsync(TokenKey, cancellationToken);

        public Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default) =>
            GetAsync(RefreshTokenKey, cancellationToken);

        public Task SetRefreshTokenAsync(string refreshToken, bool persistent = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(refreshToken);
            return SetAsync(RefreshTokenKey, refreshToken, persistent, cancellationToken);
        }

        public Task RemoveRefreshTokenAsync(CancellationToken cancellationToken = default) =>
            RemoveAsync(RefreshTokenKey, cancellationToken);

        private async Task<string?> GetAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var persistedValue = await _localStorage
                .GetItemAsync<string?>(key, cancellationToken)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(persistedValue))
                return persistedValue;

            return await _sessionStorage
                .GetItemAsync<string?>(key, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task SetAsync(string key, string value, bool persistent, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (persistent)
            {
                await _localStorage.SetItemAsync(key, value, cancellationToken).ConfigureAwait(false);
                await _sessionStorage.RemoveItemAsync(key, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _sessionStorage.SetItemAsync(key, value, cancellationToken).ConfigureAwait(false);
                await _localStorage.RemoveItemAsync(key, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _sessionStorage
                .RemoveItemAsync(key, cancellationToken)
                .ConfigureAwait(false);
            await _localStorage
                .RemoveItemAsync(key, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
