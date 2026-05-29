using Common.Http;

namespace MealPlanner.UI.Mobile.Services
{
    public sealed class SecureStorageTokenProvider : ITokenProvider
    {
        private const string TokenKey = "authToken";

        public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await SecureStorage.Default.GetAsync(TokenKey).WaitAsync(cancellationToken);
        }

        public async Task SetTokenAsync(string token, CancellationToken cancellationToken = default)
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
    }
}
