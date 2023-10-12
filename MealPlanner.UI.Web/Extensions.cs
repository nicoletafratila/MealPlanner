using Microsoft.JSInterop;

namespace MealPlanner.UI.Web
{
    public static class Extensions
    {
        public static ValueTask<bool> Confirm(this IJSRuntime jsRuntime, string message)
        {
            return jsRuntime.InvokeAsync<bool>("confirm", message);
        }
    }
}
