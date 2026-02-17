using Blazored.SessionStorage;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Services.Identities
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

            await sessionStorage.SetItemAsync(key, json);
        }

        public static async Task<TItem?> GetItemAsync<TItem>(
            this ISessionStorageService sessionStorage,
            string? name = null)
        {
            ArgumentNullException.ThrowIfNull(sessionStorage);

            var key = GetKey<TItem>(name);
            var json = await sessionStorage.GetItemAsync<string?>(key);

            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonConvert.DeserializeObject<TItem>(json, JsonSettings);
        }
    }
}