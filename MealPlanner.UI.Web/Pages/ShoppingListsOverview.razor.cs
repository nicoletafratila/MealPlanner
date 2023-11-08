using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingListsOverview
    {
        public PagedList<ShoppingListModel>? ShoppingLists { get; set; }
        public ShoppingListModel? ShoppingList { get; set; }

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [Parameter]
        public QueryParameters? QueryParameters { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        protected void New()
        {
            NavigationManager!.NavigateTo($"shoppinglistedit/");
        }

        protected void Update(ShoppingListModel item)
        {
            NavigationManager!.NavigateTo($"shoppinglistedit/{item.Id}");
        }

        protected async Task DeleteAsync(ShoppingListModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the shopping list: '{item.Name}'?"))
                    return;

                await ShoppingListService!.DeleteAsync(item.Id);
                await RefreshAsync();
            }
        }

        protected async Task RefreshAsync()
        {
            ShoppingLists = await ShoppingListService!.SearchAsync(QueryParameters!);
            StateHasChanged();
        }

        private async Task OnPageChangedAsync(int pageNumber)
        {
            QueryParameters!.PageNumber = pageNumber;
            await RefreshAsync();
        }
    }
}
