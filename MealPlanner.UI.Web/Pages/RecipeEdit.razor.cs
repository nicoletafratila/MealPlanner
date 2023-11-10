using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeEdit
    {
        [Parameter]
        public string? Id { get; set; }

        private string? _recipeCategoryId;
        public string? RecipeCategoryId
        {
            get
            {
                return _recipeCategoryId;
            }
            set
            {
                if (_recipeCategoryId != value)
                {
                    _recipeCategoryId = value;
                    Recipe!.RecipeCategoryId = int.Parse(_recipeCategoryId!);
                }
            }
        }

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
                    OnProductChanged(_productId!);
                }
            }
        }

        private string? _quantity;
        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the ingredient must be a positive number.")]
        public string? Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    StateHasChanged();
                }
            }
        }

        public EditRecipeModel? Recipe { get; set; }
        public RecipeIngredientModel? RecipeIngredient { get; set; }
        public PagedList<ProductModel>? Products { get; set; }
        public IList<RecipeCategoryModel>? RecipeCategories { get; set; }
        public IList<ProductCategoryModel>? ProductCategories { get; set; }
        private long maxFileSize = 1024L * 1024L * 1024L * 3L;

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

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            RecipeCategories = await RecipeCategoryService!.GetAllAsync();
            ProductCategories = await ProductCategoryService!.GetAllAsync();

            if (id == 0)
            {
                Recipe = new EditRecipeModel();
            }
            else
            {
                Recipe = await RecipeService!.GetEditAsync(int.Parse(Id!));
            }

            RecipeCategoryId = Recipe!.RecipeCategoryId.ToString();
        }

        protected void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/recipesoverview");
        }

        protected async Task SaveAsync()
        {
            var response = Recipe!.Id == 0 ? await RecipeService!.AddAsync(Recipe) : await RecipeService!.UpdateAsync(Recipe);
            if (!string.IsNullOrWhiteSpace(response))
            {
                ErrorComponent!.ShowError("Error", response);
            }
            else
            {
                NavigateToOverview();
            }
        }

        protected async Task DeleteAsync()
        {
            if (Recipe!.Id != 0)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the recipe: '{Recipe.Name}'?"))
                    return;

                var result = await RecipeService!.DeleteAsync(Recipe.Id);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    ErrorComponent!.ShowError("Error", result);
                }
                else
                {
                    NavigateToOverview();
                }
            }
        }

        protected bool CanAddIngredient
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ProductId) &&
                       ProductId != "0" &&
                       !string.IsNullOrWhiteSpace(Quantity) &&
                       decimal.Parse(Quantity) > 0;
            }
        }

        protected void AddIngredient()
        {
            if (!string.IsNullOrWhiteSpace(ProductId) && ProductId != "0")
            {
                if (Recipe != null)
                {
                    if (Recipe.Ingredients ==null)
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
                        item = new RecipeIngredientModel();
                        item.Product = Products!.Items!.FirstOrDefault(i => i.Id == int.Parse(ProductId));
                        item.RecipeId = Recipe.Id;
                        item.Quantity = decimal.Parse(Quantity!);
                        Recipe.Ingredients!.Add(item);
                        Quantity = string.Empty;
                    }
                }
            }
        }

        protected async Task DeleteIngredientAsync(ProductModel item)
        {
            RecipeIngredientModel? itemToDelete = Recipe!.Ingredients!.FirstOrDefault(i => i.Product!.Id == item.Id);
            if (itemToDelete != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the ingredient '{item.Name}'?"))
                    return;

                Recipe.Ingredients!.Remove(itemToDelete);
            }
        }

        private async void OnProductCategoryChangedAsync(string value)
        {
            ProductCategoryId = value;
            ProductId = string.Empty;
            Quantity = string.Empty;
            Products = await ProductService!.SearchAsync(ProductCategoryId);
            StateHasChanged();
        }

        private void OnProductChanged(string value)
        {
            ProductId = value;
            Quantity = string.Empty;
            StateHasChanged();
        }

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            try
            {
                if (e.File != null)
                {
                    Stream stream = e.File.OpenReadStream(maxAllowedSize: 1024 * 300);
                    MemoryStream ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    stream.Close();
                    Recipe!.ImageContent = ms.ToArray();
                }
                StateHasChanged();
            }
            catch (Exception)
            {
                ErrorComponent!.ShowError("Error", $"File size exceeds the limit. Maximum allowed size is <strong>{maxFileSize / (1024 * 1024)} MB</strong>.");
                return;
            }
        }
    }
}
