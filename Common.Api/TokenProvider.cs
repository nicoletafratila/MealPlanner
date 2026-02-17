using Blazored.SessionStorage;

namespace Common.Api
{
    public class TokenProvider(ISessionStorageService sessionStorage)
    {
        private readonly ISessionStorageService _sessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));
        private const string TokenKey = Constants.MealPlanner.AuthToken;

        public async Task<string?> GetTokenAsync()
        {
            return await _sessionStorage.GetItemAsync<string?>(TokenKey);
        }

        public async Task SetTokenAsync(string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            await _sessionStorage.SetItemAsync(TokenKey, token);
        }

        public async Task RemoveTokenAsync()
        {
            await _sessionStorage.RemoveItemAsync(TokenKey);
        }
    }
}
