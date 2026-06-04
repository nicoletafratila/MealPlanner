using System.ComponentModel.DataAnnotations;using BlazorBootstrap; using Common.Models; using Common.Pagination; using Common.UI; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Components.Forms; using Microsoft.AspNetCore.Components; using Microsoft.JSInterop; using RecipeBook.Services.Http; using RecipeBook.Shared.Models;

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

        [Range(0, int.MaxValue, ErrorMessageResourceType = typeof(Resources.RecipeEdit), ErrorMessageResourceName = "QuantityPositiveNumber")]
        public string? Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.RecipeEdit), ErrorMessageResourceName = "SelectUnitOfMeasurement")]
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
                new() { Text = Resources.RecipeEdit.BreadcrumbRecipes, Href = "recipebooks/recipesoverview" },
                new() { Text = Resources.RecipeEdit.BreadcrumbRecipe, IsCurrentPage = true },
            ];

            var queryParametersRecipe = new QueryParameters<RecipeCategoryModel>
            {
                Filters = [],
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "DisplaySequence",
                        Direction = Common.Pagination.SortDirection.Ascending
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
                        Direction = Common.Pagination.SortDirection.Ascending
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
                await ShowErrorAsync(Resources.RecipeEdit.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.RecipeEdit.SaveFailed);
                return;
            }

            await ShowInfoAsync(Resources.RecipeEdit.SaveSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (Recipe is null || Recipe.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.RecipeEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.RecipeEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.RecipeEdit.DeleteDialogTitle,
                message1: Resources.RecipeEdit.DeleteDialogMessage1,
                message2: Resources.RecipeEdit.DeleteDialogMessage2,
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
                await ShowErrorAsync(Resources.RecipeEdit.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.RecipeEdit.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.RecipeEdit.DeleteSucceeded);
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
                if (existing.Unit!.Id == Guid.Parse(UnitId!))
                {
                    existing.Quantity += decimal.Parse(Quantity!);
                }
                else
                {
                    await ShowErrorAsync(Resources.RecipeEdit.DuplicateIngredientError);
                }

                return;
            }

            var ingredient = new RecipeIngredientEditModel
            {
                Index = Recipe.Ingredients.Count + 1,
                RecipeId = Recipe.Id,
                Product = Products?.Items?.FirstOrDefault(i => i.Id == productId),
                Quantity = decimal.Parse(Quantity!),
                UnitId = Guid.Parse(UnitId!),
                Unit = Units?.FirstOrDefault(i => i.Id == Guid.Parse(UnitId!))
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
                YesButtonText = Resources.RecipeEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.RecipeEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.RecipeEdit.DeleteDialogTitle,
                message1: Resources.RecipeEdit.DeleteDialogMessage1,
                message2: Resources.RecipeEdit.DeleteDialogMessage2,
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
            var filters = new List<Common.Pagination.FilterItem>();

            if (!string.IsNullOrWhiteSpace(productCategoryId))
            {
                filters.Add(new Common.Pagination.FilterItem(
                    "ProductCategoryId",
                    productCategoryId,
                    Common.Pagination.FilterOperator.Equals,
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
                        Direction = Common.Pagination.SortDirection.Ascending
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
                await ShowErrorAsync(string.Format(Resources.RecipeEdit.FileSizeExceeded, _maxFileSize / (1024 * 1024)));
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