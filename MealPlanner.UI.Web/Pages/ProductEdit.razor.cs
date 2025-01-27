using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductEdit
    {
        private List<BreadcrumbItem>? NavItems { get; set; }
        private readonly long maxFileSize = 1024L * 1024L * 1024L * 3L;

        [Parameter]
        public string? Id { get; set; }
        public ProductEditModel? Product { get; set; }

        public PagedList<ProductCategoryModel>? Categories { get; set; }
        public PagedList<UnitModel>? Units { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public IProductCategoryService? CategoryService { get; set; }

        [Inject]
        public IUnitService? UnitService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            NavItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Products", Href ="/productsoverview" },
                new BreadcrumbItem{ Text = "Product", IsCurrentPage = true },
            };

            _ = int.TryParse(Id, out var id);
            var queryParameters = new QueryParameters()
            {
                Filters = new List<FilterItem>(),
                SortString = "Name",
                SortDirection = SortDirection.Ascending,
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            Units = await UnitService!.SearchAsync(queryParameters);
            Categories = await CategoryService!.SearchAsync(queryParameters);

            if (id == 0)
            {
                Product = new ProductEditModel();
            }
            else
            {
                Product = await ProductService!.GetEditAsync(id);
            }
        }

        private async void SaveAsync()
        {
            var response = Product?.Id == 0 ? await ProductService!.AddAsync(Product) : await ProductService!.UpdateAsync(Product!);
            if (!string.IsNullOrWhiteSpace(response))
            {
                MessageComponent?.ShowError(response);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async void DeleteAsync()
        {
            if (Product?.Id != 0)
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

                var response = await ProductService!.DeleteAsync(Product!.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo("/productsoverview");
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
                MessageComponent?.ShowError($"File size exceeds the limit. Maximum allowed size is <strong>{maxFileSize / (1024 * 1024)} MB</strong>.");
                return;
            }
        }
    }
}
