using Common.Api;
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

        private string? _ingredientCategoryId;
        public string? IngredientCategoryId
        {
            get
            {
                return _ingredientCategoryId;
            }
            set
            {
                if (_ingredientCategoryId != value)
                {
                    _ingredientCategoryId = value;
                    OnIngredientCategoryChangedAsync(_ingredientCategoryId!);
                }
            }
        }

        private string? _ingredientId;
        public string? IngredientId
        {
            get
            {
                return _ingredientId;
            }
            set
            {
                if (_ingredientId != value)
                {
                    _ingredientId = value;
                    OnIngredientChanged(_ingredientId!);
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
        public IList<ProductModel>? Products { get; set; }
        public IList<RecipeCategoryModel>? RecipeCategories { get; set; }
        public IList<ProductCategoryModel>? IngredientCategories { get; set; }

        public MarkupString AlertMessage { get; set; }
        public string? AlertClass { get; set; }
        private long maxFileSize = 1024L * 1024L * 1024L * 3L;

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoryService { get; set; }

        [Inject]
        public IProductCategoryService? IngredientCategoryService { get; set; }

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            RecipeCategories = await RecipeCategoryService!.GetAllAsync();
            IngredientCategories = await IngredientCategoryService!.GetAllAsync();

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

        protected async Task SaveAsync()
        {
            if (Recipe!.Id == 0)
            {
                var addedEntity = await RecipeService!.AddAsync(Recipe);
                if (addedEntity != null)
                {
                    NavigateToOverview();
                }
            }
            else
            {
                await RecipeService!.UpdateAsync(Recipe);
                NavigateToOverview();
            }
        }

        protected async Task DeleteAsync()
        {
            if (Recipe!.Id != 0)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the recipe: '{Recipe.Name}'?"))
                    return;

                await RecipeService!.DeleteAsync(Recipe.Id);
                NavigateToOverview();
            }
        }

        protected bool CanAddIngredient
        {
            get
            {
                return !string.IsNullOrWhiteSpace(IngredientId) &&
                       IngredientId != "0" &&
                       !string.IsNullOrWhiteSpace(Quantity) &&
                       decimal.Parse(Quantity) > 0;
            }
        }

        protected void AddIngredient()
        {
            if (!string.IsNullOrWhiteSpace(IngredientId) && IngredientId != "0")
            {
                if (Recipe != null)
                {
                    if (Recipe.Ingredients ==null)
                    {
                        Recipe.Ingredients = new List<RecipeIngredientModel>();
                    }
                    RecipeIngredientModel? item = Recipe.Ingredients.FirstOrDefault(i => i.Product!.Id == int.Parse(IngredientId));
                    if (item != null)
                    {
                        item.Quantity += decimal.Parse(Quantity!);
                    }
                    else
                    {
                        item = new RecipeIngredientModel();
                        item.Product = Products!.FirstOrDefault(i => i.Id == int.Parse(IngredientId));
                        item.RecipeId = Recipe.Id;
                        item.Quantity = decimal.Parse(Quantity!);
                        Recipe.Ingredients!.Add(item);

                        ClearForm();
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

        protected void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/recipesoverview");
        }

        private async void OnIngredientCategoryChangedAsync(string value)
        {
            IngredientCategoryId = value;
            IngredientId = string.Empty;
            Quantity = string.Empty;
            if (!string.IsNullOrWhiteSpace(IngredientCategoryId) && IngredientCategoryId != "0")
                Products = await ProductService!.SearchAsync(int.Parse(IngredientCategoryId));
            StateHasChanged();
        }

        private void OnIngredientChanged(string value)
        {
            IngredientId = value;
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
                    SetAlert();
                }
                StateHasChanged();
            }
            catch (Exception)
            {
                SetAlert("alert alert-danger", "oi oi-ban", $"File size exceeds the limit. Maximum allowed size is <strong>{maxFileSize / (1024 * 1024)} MB</strong>.");
                return;
            }
        }

        private void SetAlert(string alertClass = "", string iconClass = "", string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                AlertMessage = new MarkupString();
                AlertClass = string.Empty;
            }
            {
                AlertMessage = new MarkupString($"<span class='{iconClass}' aria-hidden='true'></span> {message}");
                AlertClass = alertClass;
            }
        }

        private void ClearForm()
        {
            Quantity = string.Empty;
        }
    }
}
