using Blazored.SessionStorage;

namespace Common.Api
{
    public class TokenProvider(ISessionStorageService sessionStorage)
    {
        public async Task<string> GetTokenAsync()
        {
            return await sessionStorage.GetItemAsync<string>(Common.Constants.MealPlanner.AuthToken);
        }

        public async Task SetTokenAsync(string token)
        {
            await sessionStorage.SetItemAsync(Common.Constants.MealPlanner.AuthToken, token);
        }
    }
}
