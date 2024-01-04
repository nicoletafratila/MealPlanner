﻿using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShoppingListsOverview
    {
        [Parameter]
        public QueryParameters? QueryParameters { get; set; } = new();

        public ShoppingListModel? ShoppingList { get; set; }
        public PagedList<ShoppingListModel>? ShoppingLists { get; set; }

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager!.NavigateTo($"shoppinglistedit/");
        }

        private void Update(ShoppingListModel item)
        {
            NavigationManager!.NavigateTo($"shoppinglistedit/{item.Id}");
        }

        private async Task DeleteAsync(ShoppingListModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the shopping list: '{item.Name}'?"))
                    return;

                var response = await ShoppingListService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError("Error", response);
                }
                else
                {
                    await RefreshAsync();
                }
            }
        }

        private async Task RefreshAsync()
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
