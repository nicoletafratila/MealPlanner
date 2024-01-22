﻿using BlazorBootstrap;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopsOverview
    {
        public EditShopModel? Shop { get; set; }
        public IList<ShopModel>? Shops { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager!.NavigateTo($"shopedit/");
        }

        private void Update(ShopModel item)
        {
            NavigationManager!.NavigateTo($"shopedit/{item.Id}");
        }

        private async void DeleteAsync(ShopModel item)
        {
            if (item != null)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await ShopService!.DeleteAsync(item.Id);
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
            Shops = await ShopService!.GetAllAsync();
            StateHasChanged();
        }

        //private async void OnPageChangedAsync(int pageNumber)
        //{
        //    QueryParameters!.PageNumber = pageNumber;
        //    await RefreshAsync();
        //}
    }
}
