using Common.Http;

namespace MealPlanner.UI.Mobile.Services
{
    public class SecureStorageTokenProvider : ITokenProvider
    {
        private const string TokenKey = "authToken";
        private const string RefreshTokenKey = "refreshToken";

        public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await SecureStorage.Default.GetAsync(TokenKey).WaitAsync(cancellationToken);
        }

        public async Task SetTokenAsync(string token, bool persistent = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            cancellationToken.ThrowIfCancellationRequested();
            await SecureStorage.Default.SetAsync(TokenKey, token).WaitAsync(cancellationToken);
        }

        public Task RemoveTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SecureStorage.Default.Remove(TokenKey);
            return Task.CompletedTask;
        }

        public async Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await SecureStorage.Default.GetAsync(RefreshTokenKey).WaitAsync(cancellationToken);
        }

        public async Task SetRefreshTokenAsync(string refreshToken, bool persistent = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(refreshToken);
            cancellationToken.ThrowIfCancellationRequested();
            await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken).WaitAsync(cancellationToken);
        }

        public Task RemoveRefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SecureStorage.Default.Remove(RefreshTokenKey);
            return Task.CompletedTask;
        }
    }
}
