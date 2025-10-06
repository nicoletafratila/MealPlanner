using Blazored.SessionStorage;

namespace Common.Api
{
    public class TokenProvider(ISessionStorageService sessionStorage)
    {
        public async Task<string> GetTokenAsync()
        {
            return await sessionStorage.GetItemAsync<string>(Constants.MealPlanner.AuthToken);
        }

        public async Task SetTokenAsync(string token)
        {
            await sessionStorage.SetItemAsync(Constants.MealPlanner.AuthToken, token);
        }

        public async Task RemoveTokenAsync()
        {
            await sessionStorage.RemoveItemAsync(Constants.MealPlanner.AuthToken);
        }
    }
}
