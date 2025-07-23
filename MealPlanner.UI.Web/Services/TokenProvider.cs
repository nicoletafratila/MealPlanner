using Blazored.LocalStorage;

namespace MealPlanner.UI.Web.Services
{
    public class TokenProvider
    {
        private readonly ILocalStorageService localStorage;
        public TokenProvider(ILocalStorageService localStorage) => this.localStorage = localStorage;

        public async Task StoreTokensAsync(string token)
        {
            await localStorage.SetItemAsync("access_token", token);
        }
        public async Task<string> GetAccessTokenAsync()
        {
            //return await localStorage.GetItemAsync<string>("access_token");
            return string.Empty;
        }


        //    if (msg.IsSuccessStatusCode)
        //{
        //	LoginResult result = await msg.Content.ReadFromJsonAsync<LoginResult>();
        //    message = result.message;
        //	isDisabled = false;
        //	if (result.success)
        //                    await jsr.InvokeVoidAsync("localStorage.setItem", "user", $"{result.email};{result.jwtBearer}").ConfigureAwait(false);
    }
}
