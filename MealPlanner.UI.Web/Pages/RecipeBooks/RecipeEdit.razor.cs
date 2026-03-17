using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipeEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private readonly long _maxFileSize = 1024L * 1024L * 3L;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public RecipeEditModel? Recipe { get; set; }

        public PagedList<RecipeCategoryModel>? RecipeCategories { get; set; }

        public PagedList<ProductCategoryModel>? ProductCategories { get; set; }

        public string? ProductId { get; set; }
        public PagedList<ProductModel>? Products { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the ingredient must be a positive number.")]
        public string? Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the ingredient.")]
        public string? UnitId { get; set; }

        public IList<UnitModel>? Units { get; set; }
        public PagedList<UnitModel>? BaseUnits { get; set; }

        [Inject]
        public IRecipeService RecipeService { get; set; } = default!;

        [Inject]
        public IRecipeCategoryService RecipeCategoryService { get; set; } = default!;

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; } = default!;

        [Inject]
        public IProductService ProductService { get; set; } = default!;

        [Inject]
        public IUnitService UnitService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new() { Text = "Recipes", Href = "recipebooks/recipesoverview" },
                new() { Text = "Recipe", IsCurrentPage = true },
            ];

            var queryParametersRecipe = new QueryParameters<RecipeCategoryModel>
            {
                Filters = [],
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "DisplaySequence",
                        Direction = SortDirection.Ascending
                    }
                ],
                PageSize = int.MaxValue,
                PageNumber = 1
            };

            RecipeCategories = await RecipeCategoryService.SearchAsync(queryParametersRecipe);

            var queryParametersProduct = new QueryParameters<ProductCategoryModel>
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

            ProductCategories = await ProductCategoryService.SearchAsync(queryParametersProduct);
            BaseUnits = await UnitService.SearchAsync();

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                Recipe = new RecipeEditModel();
            }
            else
            {
                Recipe = await RecipeService.GetEditAsync(id)
                          ?? new RecipeEditModel { Id = id };
            }
        }

        private async Task SaveAsync()
        {
            if (Recipe is null)
                return;

            await SaveCoreAsync(Recipe);
        }

        private async Task SaveCoreAsync(RecipeEditModel recipe)
        {
            CommandResponse? response;

            if (recipe.Id == 0)
            {
                response = await RecipeService.AddAsync(recipe);
            }
            else
            {
                response = await RecipeService.UpdateAsync(recipe);
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
            if (Recipe is null || Recipe.Id == 0)
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
                message1: "This will delete the record. Once deleted can not be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(Recipe);
        }

        private async Task DeleteCoreAsync(RecipeEditModel recipe)
        {
            if (recipe.Id == 0)
                return;

            var response = await RecipeService.DeleteAsync(recipe.Id);
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

        private bool CanAddIngredient =>
            !string.IsNullOrWhiteSpace(ProductId) &&
            ProductId != "0" &&
            UnitId != "0" &&
            !string.IsNullOrWhiteSpace(Quantity) &&
            double.TryParse(Quantity, out var quantityValue) &&
            quantityValue > 0;

        private async Task AddIngredientAsync()
        {
            if (Recipe is null || string.IsNullOrWhiteSpace(ProductId) || ProductId == "0")
                return;

            if (Recipe.Ingredients == null)
            {
                Recipe.Ingredients = [];
            }

            var productId = int.Parse(ProductId!);

            var existing = Recipe.Ingredients.FirstOrDefault(i => i.Product?.Id == productId);
            if (existing != null)
            {
                if (existing.Unit!.Id == int.Parse(UnitId!))
                {
                    existing.Quantity += decimal.Parse(Quantity!);
                }
                else
                {
                    await ShowErrorAsync("The same ingredient was added to the recipe with a different unit of measurement.");
                }

                return;
            }

            var ingredient = new RecipeIngredientEditModel
            {
                Index = Recipe.Ingredients.Count + 1,
                RecipeId = Recipe.Id,
                Product = Products?.Items?.FirstOrDefault(i => i.Id == productId),
                Quantity = decimal.Parse(Quantity!),
                UnitId = int.Parse(UnitId!),
                Unit = Units?.FirstOrDefault(i => i.Id == int.Parse(UnitId!))
            };

            Recipe.Ingredients.Add(ingredient);

            Quantity = string.Empty;
            UnitId = string.Empty;
        }

        private async Task DeleteIngredientAsync(ProductModel item)
        {
            if (Recipe?.Ingredients is null)
                return;

            var itemToDelete = Recipe.Ingredients.FirstOrDefault(i => i.Product?.Id == item.Id);
            if (itemToDelete is null)
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
                message1: "This will delete the record. Once deleted can not be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            Recipe.Ingredients.Remove(itemToDelete);
            StateHasChanged();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("recipebooks/recipesoverview");
        }

        private async Task OnProductCategoryChangedAsync(ChangeEventArgs e)
        {
            var productCategoryId = e.Value?.ToString();
            var filters = new List<FilterItem>();

            if (!string.IsNullOrWhiteSpace(productCategoryId))
            {
                filters.Add(new FilterItem(
                    "ProductCategoryId",
                    productCategoryId,
                    FilterOperator.Equals,
                    StringComparison.OrdinalIgnoreCase));
            }

            var queryParameters = new QueryParameters<ProductModel>
            {
                Filters = filters,
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

            Products = await ProductService.SearchAsync(queryParameters);

            ProductId = string.Empty;
            Quantity = string.Empty;

            StateHasChanged();
        }

        private async Task OnProductChangedAsync(ChangeEventArgs e)
        {
            var productId = e.Value?.ToString();
            ProductId = productId;
            Quantity = string.Empty;

            if (string.IsNullOrWhiteSpace(productId))
            {
                StateHasChanged();
                return;
            }

            var product = await ProductService.GetEditAsync(int.Parse(productId));
            if (product == null || BaseUnits?.Items == null)
            {
                StateHasChanged();
                return;
            }

            var baseUnit = BaseUnits.Items.FirstOrDefault(x => x.Id == product.BaseUnitId);
            if (baseUnit == null)
            {
                StateHasChanged();
                return;
            }

            Units = BaseUnits.Items
                .Where(x => x.UnitType == baseUnit.UnitType)
                .ToList();

            StateHasChanged();
        }

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            try
            {
                if (e.File != null && Recipe != null)
                {
                    await using var stream = e.File.OpenReadStream(maxAllowedSize: _maxFileSize);
                    await using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    Recipe.ImageContent = ms.ToArray();
                }

                StateHasChanged();
            }
            catch (Exception)
            {
                await ShowErrorAsync($"File size exceeds the limit. Maximum allowed size is <strong>{_maxFileSize / (1024 * 1024)} MB</strong>.");
            }
        }

        private async Task CheckQuantityAsync(ChangeEventArgs _)
        {
            await JS.InvokeVoidAsync("checkQuantity");
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}