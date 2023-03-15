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
        public string Id { get; set; }

        private string _recipeCategoryId;
        public string RecipeCategoryId
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
                    Recipe.RecipeCategoryId = int.Parse(_recipeCategoryId);
                }
            }
        }

        private string _ingredientCategoryId;
        public string IngredientCategoryId
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
                    OnIngredientCategoryChangedAsync(_ingredientCategoryId);
                }
            }
        }

        private string _ingredientId;
        public string IngredientId
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
                    OnIngredientChanged(_ingredientId);
                }
            }
        }

        private string _quantity;
        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the ingredient must be a positive number.")]
        public string Quantity
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

        public EditRecipeModel Recipe { get; set; } = new EditRecipeModel();
        public RecipeIngredientModel RecipeIngredient { get; set; } = new RecipeIngredientModel();
        public IList<IngredientModel> Ingredients { get; set; } = new List<IngredientModel>();
        public IList<RecipeCategoryModel> RecipeCategories { get; set; } = new List<RecipeCategoryModel>();
        public IList<IngredientCategoryModel> IngredientCategories { get; set; } = new List<IngredientCategoryModel>();

        [Inject]
        public IRecipeService RecipeService { get; set; }

        [Inject]
        public IRecipeCategoryService RecipeCategoryService { get; set; }

        [Inject]
        public IIngredientCategoryService IngredientCategoryService { get; set; }

        [Inject]
        public IIngredientService IngredientService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            RecipeCategories = await RecipeCategoryService.GetAllAsync();
            IngredientCategories = await IngredientCategoryService.GetAllAsync();

            if (id == 0)
            {
                Recipe = new EditRecipeModel();
            }
            else
            {
                Recipe = await RecipeService.GetEditAsync(int.Parse(Id));
            }

            RecipeCategoryId = Recipe.RecipeCategoryId.ToString();
        }

        protected async Task SaveAsync()
        {
            if (Recipe.Id == 0)
            {
                var addedEntity = await RecipeService.AddAsync(Recipe);
                if (addedEntity != null)
                {
                    NavigateToOverview();
                }
            }
            else
            {
                await RecipeService.UpdateAsync(Recipe);
                NavigateToOverview();
            }
        }

        protected async Task DeleteAsync()
        {
            if (Recipe.Id != 0)
            {
                if (!await JSRuntime.Confirm($"Are you sure you want to delete the recipe: '{Recipe.Name}'?"))
                    return;

                await RecipeService.DeleteAsync(Recipe.Id);
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
                RecipeIngredientModel item = Recipe.Ingredients.FirstOrDefault(i => i.Ingredient.Id == int.Parse(IngredientId));
                if (item != null)
                {
                    item.Quantity += decimal.Parse(Quantity);
                }
                else
                {
                    item = new RecipeIngredientModel();
                    item.Ingredient = Ingredients.FirstOrDefault(i => i.Id == int.Parse(IngredientId));
                    item.RecipeId = Recipe.Id;
                    item.Quantity = decimal.Parse(Quantity);
                    Recipe.Ingredients.Add(item);

                    ClearForm();
                }
            }
        }

        protected async Task DeleteIngredientAsync(IngredientModel item)
        {
            RecipeIngredientModel itemToDelete = Recipe.Ingredients.FirstOrDefault(i => i.Ingredient.Id == item.Id);
            if (itemToDelete != null)
            {
                if (!await JSRuntime.Confirm($"Are you sure you want to delete the ingredient '{item.Name}'?"))
                    return;

                Recipe.Ingredients.Remove(itemToDelete);
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/recipesoverview");
        }

        private async void OnIngredientCategoryChangedAsync(string value)
        {
            IngredientCategoryId = value;
            IngredientId = string.Empty;
            Quantity = string.Empty;
            if (!string.IsNullOrWhiteSpace(IngredientCategoryId) && IngredientCategoryId != "0")
                Ingredients = await IngredientService.SearchAsync(int.Parse(IngredientCategoryId));
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
                var _selectedFiles = e.GetMultipleFiles();
                if (_selectedFiles != null)
                {
                    var file = _selectedFiles[0];
                    Stream stream = file.OpenReadStream();
                    MemoryStream ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    stream.Close();
                    Recipe.ImageContent = ms.ToArray();
                }
                StateHasChanged();
            }
            catch(Exception ex)
            {
            }
            StateHasChanged();
        }

        private void ClearForm()
        {
            Quantity = string.Empty;
        }
    }
}
