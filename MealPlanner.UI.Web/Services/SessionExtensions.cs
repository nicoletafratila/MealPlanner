using Blazored.SessionStorage;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Services
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None
        };

        private static string GetKey<TItem>(string? name) =>
            string.IsNullOrWhiteSpace(name)
                ? (typeof(TItem).FullName ?? typeof(TItem).Name)
                : name;

        public static async Task SetItemAsync<TItem>(
            this ISessionStorageService sessionStorage,
            TItem info,
            string? name = null)
        {
            ArgumentNullException.ThrowIfNull(sessionStorage);

            var key = GetKey<TItem>(name);
            var json = JsonConvert.SerializeObject(info, JsonSettings);

            try
            {
                await sessionStorage.SetItemAsync(key, json);
            }
            catch (JSDisconnectedException)
            {
            }
        }

        public static async Task<TItem?> GetItemAsync<TItem>(
            this ISessionStorageService sessionStorage,
            string? name = null)
        {
            ArgumentNullException.ThrowIfNull(sessionStorage);

            var key = GetKey<TItem>(name);

            string? json;
            try
            {
                json = await sessionStorage.GetItemAsync<string?>(key);
            }
            catch (JSDisconnectedException)
            {
                return default;
            }

            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonConvert.DeserializeObject<TItem>(json, JsonSettings);
        }
    }
}
