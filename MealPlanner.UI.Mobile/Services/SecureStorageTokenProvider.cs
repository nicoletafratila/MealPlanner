using Common.Http;

namespace MealPlanner.UI.Mobile.Services
{
    public class SecureStorageTokenProvider : ITokenProvider
    {
        private const string TokenKey = "authToken";
        private const string UsernameKey = "savedUsername";
        private const string PasswordKey = "savedPassword";

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

        public async Task SaveCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            await SecureStorage.Default.SetAsync(UsernameKey, username).WaitAsync(cancellationToken);
            await SecureStorage.Default.SetAsync(PasswordKey, password).WaitAsync(cancellationToken);
        }

        public async Task<(string? Username, string? Password)> GetCredentialsAsync(CancellationToken cancellationToken = default)
        {
            var username = await SecureStorage.Default.GetAsync(UsernameKey).WaitAsync(cancellationToken);
            var password = await SecureStorage.Default.GetAsync(PasswordKey).WaitAsync(cancellationToken);
            return (username, password);
        }

        public void ClearCredentials()
        {
            SecureStorage.Default.Remove(UsernameKey);
            SecureStorage.Default.Remove(PasswordKey);
        }
    }
}
