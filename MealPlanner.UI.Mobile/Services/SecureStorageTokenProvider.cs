using Common.Http;

namespace MealPlanner.UI.Mobile.Services
{
    public class SecureStorageTokenProvider : ITokenProvider
    {
        private const string TokenKey = "authToken";
        private const string RefreshTokenKey = "refreshToken";

        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private readonly SemaphoreSlim _refreshTokenLock = new(1, 1);

        private string? _cachedToken;
        private bool _tokenCached;
        private string? _cachedRefreshToken;
        private bool _refreshTokenCached;

        public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_tokenCached)
                return _cachedToken;

            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (!_tokenCached)
                {
                    _cachedToken = await SecureStorage.Default.GetAsync(TokenKey).WaitAsync(cancellationToken);
                    _tokenCached = true;
                }
                return _cachedToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        public async Task SetTokenAsync(string token, bool persistent = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            cancellationToken.ThrowIfCancellationRequested();
            await SecureStorage.Default.SetAsync(TokenKey, token).WaitAsync(cancellationToken);
            _cachedToken = token;
            _tokenCached = true;
        }

        public Task RemoveTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SecureStorage.Default.Remove(TokenKey);
            _cachedToken = null;
            _tokenCached = true;
            return Task.CompletedTask;
        }

        public async Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_refreshTokenCached)
                return _cachedRefreshToken;

            await _refreshTokenLock.WaitAsync(cancellationToken);
            try
            {
                if (!_refreshTokenCached)
                {
                    _cachedRefreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey).WaitAsync(cancellationToken);
                    _refreshTokenCached = true;
                }
                return _cachedRefreshToken;
            }
            finally
            {
                _refreshTokenLock.Release();
            }
        }

        public async Task SetRefreshTokenAsync(string refreshToken, bool persistent = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(refreshToken);
            cancellationToken.ThrowIfCancellationRequested();
            await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken).WaitAsync(cancellationToken);
            _cachedRefreshToken = refreshToken;
            _refreshTokenCached = true;
        }

        public Task RemoveRefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SecureStorage.Default.Remove(RefreshTokenKey);
            _cachedRefreshToken = null;
            _refreshTokenCached = true;
            return Task.CompletedTask;
        }
    }
}
