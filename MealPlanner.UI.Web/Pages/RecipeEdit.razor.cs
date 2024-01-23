using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RecipeBook.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeEdit
    {
        private readonly long maxFileSize = 1024L * 1024L * 1024L * 3L;

        [Parameter]
        public string? Id { get; set; }
        public EditRecipeModel? Recipe { get; set; }

        public IList<RecipeCategoryModel>? RecipeCategories { get; set; }

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
        public IList<ProductCategoryModel>? ProductCategories { get; set; }

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
                }
            }
        }
        public PagedList<ProductModel>? Products { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the ingredient must be a positive number.")]
        public string? Quantity { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoryService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoryService { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            _ = int.TryParse(Id, out var id);
            RecipeCategories = await RecipeCategoryService!.GetAllAsync();
            ProductCategories = await ProductCategoryService!.GetAllAsync();

            if (id == 0)
            {
                Recipe = new EditRecipeModel();
            }
            else
            {
                Recipe = await RecipeService!.GetEditAsync(id);
            }
        }

        private async void SaveAsync()
        {
            var response = Recipe!.Id == 0 ? await RecipeService!.AddAsync(Recipe) : await RecipeService!.UpdateAsync(Recipe);
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
            if (Recipe!.Id != 0)
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

                var result = await RecipeService!.DeleteAsync(Recipe.Id);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    ErrorComponent!.ShowError(result);
                }
                else
                {
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
                       !string.IsNullOrWhiteSpace(Quantity) &&
                       decimal.TryParse(Quantity, out _);
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
                        Recipe.Ingredients = new List<RecipeIngredientModel>();
                    }
                    RecipeIngredientModel? item = Recipe.Ingredients.FirstOrDefault(i => i.Product!.Id == int.Parse(ProductId));
                    if (item != null)
                    {
                        item.Quantity += decimal.Parse(Quantity!);
                    }
                    else
                    {
                        item = new RecipeIngredientModel
                        {
                            Product = Products!.Items!.FirstOrDefault(i => i.Id == int.Parse(ProductId)),
                            RecipeId = Recipe.Id,
                            Quantity = decimal.Parse(Quantity!)
                        };
                        Recipe.Ingredients!.Add(item);
                        Quantity = string.Empty;
                    }
                }
            }
        }

        private async void DeleteIngredientAsync(ProductModel item)
        {
            RecipeIngredientModel? itemToDelete = Recipe!.Ingredients!.FirstOrDefault(i => i.Product!.Id == item.Id);
            if (itemToDelete != null)
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

                Recipe.Ingredients!.Remove(itemToDelete);
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/recipesoverview");
        }

        private async void OnProductCategoryChangedAsync(string value)
        {
            ProductCategoryId = value;
            ProductId = string.Empty;
            Quantity = string.Empty;
            Products = await ProductService!.SearchAsync(ProductCategoryId);
            StateHasChanged();
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
                    Recipe!.ImageContent = ms.ToArray();
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
