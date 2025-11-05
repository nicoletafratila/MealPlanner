using Blazored.SessionStorage;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web
{
    public static class SessionExtensions
    {
        public static async Task SetItemAsync<TItem>(this ISessionStorageService sessionStorage, TItem info, string? name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = typeof(TItem).FullName;
            }
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
            };

            await sessionStorage!.SetItemAsync(name, JsonConvert.SerializeObject(info, settings));
        }

        public static async Task<TItem?> GetItemAsync<TItem>(this ISessionStorageService sessionStorage, string? name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = typeof(TItem).FullName;
            }
            var infoJson = await sessionStorage!.GetItemAsync<string>(name);
            if (string.IsNullOrWhiteSpace(infoJson))
                return default;

            return JsonConvert.DeserializeObject<TItem>(infoJson);
        }
    }
}
