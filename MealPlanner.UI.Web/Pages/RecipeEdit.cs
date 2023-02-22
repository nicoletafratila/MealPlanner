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
        private IReadOnlyList<IBrowserFile> _selectedFiles;

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
                    Model.RecipeCategoryId = int.Parse(_recipeCategoryId);
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
                    OnIngredientCategoryChanged(_ingredientCategoryId);
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

        public EditRecipeModel Model { get; set; } = new EditRecipeModel();
        public RecipeIngredientModel CurrentIngredientModel { get; set; } = new RecipeIngredientModel();
        public List<RecipeCategoryModel> RecipeCategories { get; set; } = new List<RecipeCategoryModel>();
        public List<IngredientCategoryModel> IngredientCategories { get; set; } = new List<IngredientCategoryModel>();
        public List<IngredientModel> Ingredients { get; set; } = new List<IngredientModel>();

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

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            RecipeCategories = (await RecipeCategoryService.GetAll()).ToList();
            IngredientCategories = (await IngredientCategoryService.GetAll()).ToList();

            if (id == 0)
            {
                Model = new EditRecipeModel();
            }
            else
            {
                Model = await RecipeService.GetEdit(int.Parse(Id));
            }

            RecipeCategoryId = Model.RecipeCategoryId.ToString();
        }

        protected async Task Save()
        {
            if (Model.Id == 0)
            {
                var addedEntity = await RecipeService.Add(Model);
                if (addedEntity != null)
                {
                    NavigateToOverview();
                }
            }
            else
            {
                await RecipeService.Update(Model);
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
                RecipeIngredientModel item = Model.Ingredients.FirstOrDefault(i => i.Ingredient.Id == int.Parse(IngredientId));
                if (item != null)
                {
                    item.Quantity += decimal.Parse(Quantity);
                }
                else
                {
                    item = new RecipeIngredientModel();
                    item.Ingredient = Ingredients.FirstOrDefault(i => i.Id == int.Parse(IngredientId));
                    item.RecipeId = Model.Id;
                    item.Quantity = decimal.Parse(Quantity);
                    Model.Ingredients.Add(item);

                    ClearForm();
                }
            }
        }

        protected void DeleteIngredient(IngredientModel item)
        {
            //if (!await JSRuntime.InvokeAsync<bool>("confirm", new object[] { $"Are you sure you want to delete the ingredient '{item.Name}'?" }))
            //    return;

            RecipeIngredientModel itemToDelete = Model.Ingredients.FirstOrDefault(i => i.Ingredient.Id == item.Id);
            if (itemToDelete != null)
            {
                Model.Ingredients.Remove(itemToDelete);
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/recipesoverview");
        }

        private async void OnIngredientCategoryChanged(string value)
        {
            IngredientCategoryId = value;
            IngredientId = string.Empty;
            Quantity = string.Empty;
            if (!string.IsNullOrWhiteSpace(IngredientCategoryId) && IngredientCategoryId != "0")
                Ingredients = (await IngredientService.SearchAsync(int.Parse(IngredientCategoryId))).ToList();
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
            _selectedFiles = e.GetMultipleFiles();
            if (_selectedFiles != null)
            {
                var file = _selectedFiles[0];
                Stream stream = file.OpenReadStream();
                MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                stream.Close();
                Model.ImageContent = ms.ToArray();
            }
            StateHasChanged();
        }

        private void ClearForm()
        {
            Quantity = string.Empty;
        }
    }
}
