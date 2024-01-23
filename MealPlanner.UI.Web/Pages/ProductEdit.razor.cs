﻿using BlazorBootstrap;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductEdit
    {
        private readonly long maxFileSize = 1024L * 1024L * 1024L * 3L;

        [Parameter]
        public string? Id { get; set; }
        public EditProductModel? Product { get; set; }

        public IList<ProductCategoryModel>? Categories { get; set; }
        public IList<UnitModel>? Units { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public IProductCategoryService? CategoryService { get; set; }

        [Inject]
        public IUnitService? UnitService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            _ = int.TryParse(Id, out var id);
            Categories = await CategoryService!.GetAllAsync();
            Units = await UnitService!.GetAllAsync();

            if (id == 0)
            {
                Product = new EditProductModel();
            }
            else
            {
                Product = await ProductService!.GetEditAsync(id);
            }
        }

        private async void SaveAsync()
        {
            var response = Product!.Id == 0 ? await ProductService!.AddAsync(Product) : await ProductService!.UpdateAsync(Product);
            if (!string.IsNullOrWhiteSpace(response))
            {
                ErrorComponent!.ShowError(response);
            }
            else
            {
                NavigateToOverview();
            }
        }

        private async void DeleteAsync()
        {
            if (Product!.Id != 0)
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

                var response = await ProductService!.DeleteAsync(Product.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError(response);
                }
                else
                {
                    NavigateToOverview();
                }
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/productsoverview");
        }

        private async void OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            try
            {
                if (e.File != null)
                {
                    Stream stream = e.File.OpenReadStream(maxAllowedSize: 1024 * 300);
                    MemoryStream ms = new();
                    await stream.CopyToAsync(ms);
                    stream.Close();
                    Product!.ImageContent = ms.ToArray();
                }
                StateHasChanged();
            }
            catch (Exception)
            {
                ErrorComponent!.ShowError($"File size exceeds the limit. Maximum allowed size is <strong>{maxFileSize / (1024 * 1024)} MB</strong>.");
                return;
            }
        }
    }
}
