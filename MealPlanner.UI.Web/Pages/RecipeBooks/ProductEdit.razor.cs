using BlazorBootstrap;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class ProductEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];

        // 3 MB max file size
        private readonly long _maxFileSize = 1024L * 1024L * 3L;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public ProductEditModel Product { get; set; } = new();

        public PagedList<ProductCategoryModel>? Categories { get; private set; }
        public PagedList<UnitModel>? Units { get; private set; }

        [Inject]
        public IProductService ProductService { get; set; } = default!;

        [Inject]
        public IProductCategoryService CategoryService { get; set; } = default!;

        [Inject]
        public IUnitService UnitService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = "Products", Href = "recipebooks/productsoverview" },
                new BreadcrumbItem { Text = "Product", IsCurrentPage = true },
            ];

            Units = await UnitService.SearchAsync();

            var queryParameters = new QueryParameters<ProductCategoryModel>
            {
                Filters = [],
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "Name",
                        Direction = SortDirection.Ascending
                    }
                ],
                PageSize = int.MaxValue,
                PageNumber = 1
            };

            Categories = await CategoryService.SearchAsync(queryParameters);

            if (!int.TryParse(Id, out var id) || id == 0)
            {
                Product = new ProductEditModel();
            }
            else
            {
                Product = await ProductService.GetEditAsync(id) ?? new ProductEditModel { Id = id };
            }
        }

        private async Task SaveAsync()
        {
            await SaveCoreAsync(Product);
        }

        private async Task SaveCoreAsync(ProductEditModel product)
        {
            Common.Models.CommandResponse? response;

            if (product.Id == 0)
            {
                response = await ProductService.AddAsync(product);
            }
            else
            {
                response = await ProductService.UpdateAsync(product);
            }

            if (response is null)
            {
                await ShowErrorAsync("Save failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? "Save failed.");
                return;
            }

            await ShowInfoAsync("Data has been saved successfully");
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (Product.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = "OK",
                YesButtonColor = ButtonColor.Success,
                NoButtonText = "Cancel",
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: "Are you sure you want to delete this?",
                message1: "This will delete the record. Once deleted it cannot be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(Product);
        }

        private async Task DeleteCoreAsync(ProductEditModel product)
        {
            if (product.Id == 0)
                return;

            var response = await ProductService.DeleteAsync(product.Id);
            if (response is null)
            {
                await ShowErrorAsync("Delete failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? "Delete failed.");
                return;
            }

            await ShowInfoAsync("Data has been deleted successfully");
            NavigateToOverview();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("recipebooks/productsoverview");
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            try
            {
                if (e.File is not null)
                {
                    await using var stream = e.File.OpenReadStream(maxAllowedSize: _maxFileSize);
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    Product.ImageContent = ms.ToArray();
                }

                StateHasChanged();
            }
            catch (Exception)
            {
                await ShowErrorAsync($"File size exceeds the limit. Maximum allowed size is <strong>{_maxFileSize / (1024 * 1024)} MB</strong>.");
            }
        }
    }
}
