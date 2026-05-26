using BlazorBootstrap;
using Common.Pagination;
using Common.UI;
using RecipeBook.Services;
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
                new BreadcrumbItem { Text = Resources.ProductEdit.BreadcrumbProducts, Href = "recipebooks/productsoverview" },
                new BreadcrumbItem { Text = Resources.ProductEdit.BreadcrumbProduct, IsCurrentPage = true },
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
                await ShowErrorAsync(Resources.ProductEdit.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ProductEdit.SaveFailed);
                return;
            }

            await ShowInfoAsync(Resources.ProductEdit.SaveSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (Product.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.ProductEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ProductEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ProductEdit.DeleteDialogTitle,
                message1: Resources.ProductEdit.DeleteDialogMessage1,
                message2: Resources.ProductEdit.DeleteDialogMessage2,
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
                await ShowErrorAsync(Resources.ProductEdit.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ProductEdit.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.ProductEdit.DeleteSucceeded);
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
                await ShowErrorAsync(string.Format(Resources.ProductEdit.FileSizeExceeded, _maxFileSize / (1024 * 1024)));
            }
        }
    }
}
