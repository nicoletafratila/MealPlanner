using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class RecipeEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private readonly long _maxFileSize = 1024L * 1024L * 1024L * 3L;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }
        public RecipeEditModel? Recipe { get; set; }

        public PagedList<RecipeCategoryModel>? RecipeCategories { get; set; }

        private string? _productCategoryId;
        public string? ProductCategoryId
        {
            get
            {
                return _productCategoryId;
            }
            set
            {
                if (_productCategoryId != value)
                {
                    _productCategoryId = value;
                    OnProductCategoryChangedAsync(_productCategoryId!);
                }
            }
        }
        public PagedList<ProductCategoryModel>? ProductCategories { get; set; }

        private string? _productId;
        public string? ProductId
        {
            get
            {
                return _productId;
            }
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    Quantity = string.Empty;
                    UnitId = string.Empty;
                    OnProductChangedAsync(_productId!);
                }
            }
        }
        public PagedList<ProductModel>? Products { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the ingredient must be a positive number.")]
        public string? Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the ingredient.")]
        public string? UnitId { get; set; }
        public IList<UnitModel>? Units { get; set; }
        public PagedList<UnitModel>? BaseUnits { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoryService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoryService { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public IUnitService? UnitService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Recipes", Href ="/recipesoverview" },
                new BreadcrumbItem{ Text = "Recipe", IsCurrentPage = true },
            };

            var queryParameters = new QueryParameters()
            {
                Filters = new List<FilterItem>(),
                SortString = "DisplaySequence",
                SortDirection = SortDirection.Ascending,
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            RecipeCategories = await RecipeCategoryService!.SearchAsync(queryParameters);
            ProductCategories = await ProductCategoryService!.SearchAsync();
            BaseUnits = await UnitService!.SearchAsync();

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                Recipe = new RecipeEditModel();
            }
            else
            {
                Recipe = await RecipeService!.GetEditAsync(id);
            }
        }

        private async Task SaveAsync()
        {
            var response = Recipe?.Id == 0 ? await RecipeService!.AddAsync(Recipe) : await RecipeService!.UpdateAsync(Recipe!);
            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async Task DeleteAsync()
        {
            if (Recipe?.Id != 0)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await _dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await RecipeService!.DeleteAsync(Recipe!.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private bool CanAddIngredient
        {
            get
            {

                return !string.IsNullOrWhiteSpace(ProductId) &&
                       ProductId != "0" &&
                       UnitId != "0" &&
                       !string.IsNullOrWhiteSpace(Quantity) &&
                       double.TryParse(Quantity, out double quantity1) &&
                       quantity1 > 0;
            }
        }

        private void AddIngredient()
        {
            if (!string.IsNullOrWhiteSpace(ProductId) && ProductId != "0")
            {
                if (Recipe != null)
                {
                    if (Recipe.Ingredients == null)
                    {
                        Recipe.Ingredients = new List<RecipeIngredientEditModel>();
                    }
                    RecipeIngredientEditModel? item = Recipe.Ingredients.FirstOrDefault(i => i.Product?.Id == int.Parse(ProductId));
                    if (item != null)
                    {
                        if (item.Unit!.Id == int.Parse(UnitId!))
                        {
                            item.Quantity += decimal.Parse(Quantity!);
                        }
                        else
                        {
                            MessageComponent?.ShowError("The same ingredient was added to the recipe with a different unit of measurement.");
                        }
                    }
                    else
                    {
                        item = new RecipeIngredientEditModel
                        {
                            Index = Recipe.Ingredients.Count + 1,
                            RecipeId = Recipe.Id,
                            Product = Products?.Items?.FirstOrDefault(i => i.Id == int.Parse(ProductId)),
                            Quantity = decimal.Parse(Quantity!),
                            UnitId = int.Parse(UnitId!),
                            Unit = Units?.FirstOrDefault(i => i.Id == int.Parse(UnitId!))
                        };
                        Recipe.Ingredients?.Add(item);
                        Quantity = string.Empty;
                        UnitId = string.Empty;
                    }
                }
            }
        }

        private async Task DeleteIngredientAsync(ProductModel item)
        {
            RecipeIngredientEditModel? itemToDelete = Recipe?.Ingredients?.FirstOrDefault(i => i.Product?.Id == item.Id);
            if (itemToDelete != null)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await _dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                Recipe?.Ingredients?.Remove(itemToDelete);
                StateHasChanged();
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo("/recipesoverview");
        }

        private async Task OnProductCategoryChangedAsync(string value)
        {
            var filters = new List<FilterItem>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                filters.Add(new FilterItem("ProductCategoryId", value, FilterOperator.Equals, StringComparison.OrdinalIgnoreCase));
            };
            var queryParameters = new QueryParameters()
            {
                Filters = filters,
                SortString = "Name",
                SortDirection = SortDirection.Ascending,
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            Products = await ProductService!.SearchAsync(queryParameters);

            ProductCategoryId = value;
            ProductId = string.Empty;
            Quantity = string.Empty;
            StateHasChanged();
        }

        private async Task OnProductChangedAsync(string value)
        {
            Quantity = string.Empty;
            if (!string.IsNullOrWhiteSpace(value))
            {
                var product = await ProductService!.GetEditAsync(int.Parse(value));
                if (product != null)
                {
                    var baseUnit = BaseUnits!.Items!.FirstOrDefault(x => x.Id == product.BaseUnitId);
                    Units = BaseUnits!.Items!.Where(x => x.UnitType == baseUnit!.UnitType).ToList();
                }
            }
            StateHasChanged();
        }

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            try
            {
                if (e.File != null)
                {
                    Stream stream = e.File.OpenReadStream(maxAllowedSize: 1024 * 300);
                    MemoryStream ms = new();
                    await stream.CopyToAsync(ms);
                    stream.Close();
                    Recipe!.ImageContent = ms.ToArray();
                }
                StateHasChanged();
            }
            catch (Exception)
            {
                MessageComponent?.ShowError($"File size exceeds the limit. Maximum allowed size is <strong>{_maxFileSize / (1024 * 1024)} MB</strong>.");
                return;
            }
        }

        private async Task CheckQuantityAsync(ChangeEventArgs e)
        {
            await JS.InvokeVoidAsync("checkQuantity");
        }
    }
}
