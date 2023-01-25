using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeEdit
    {
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

        [Parameter]
        public string Id { get; set; }

        protected string RecipeCategoryId = string.Empty;
        protected string IngredientId = string.Empty;
        protected string IngredientCategoryId = string.Empty;
        protected string Quantity = string.Empty;
        public EditRecipeModel Model { get; set; } = new EditRecipeModel();
        private IReadOnlyList<IBrowserFile> _selectedFiles;

        public List<RecipeCategoryModel> RecipeCategories { get; set; } = new List<RecipeCategoryModel>();
        public List<IngredientCategoryModel> IngredientCategories { get; set; } = new List<IngredientCategoryModel>();
        public List<IngredientModel> Ingredients { get; set; } = new List<IngredientModel>();

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
                Model = await RecipeService.Get(int.Parse(Id));
            }

            RecipeCategoryId = Model.RecipeCategoryId.ToString();
        }

        protected async Task HandleValidSubmit()
        {
            Model.RecipeCategoryId = int.Parse(RecipeCategoryId);
            if (_selectedFiles != null)
            {
                var file = _selectedFiles[0];
                Stream stream = file.OpenReadStream();
                MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                stream.Close();
                Model.ImageContent = ms.ToArray();
            }

            if (Model.Id == 0)
            {
                var addedEntity = await RecipeService.Add(Model);
                if (addedEntity != null)
                {
                    NavigationManager.NavigateTo("/recipesoverview");
                }
            }
            else
            {
                await RecipeService.Update(Model);
                NavigationManager.NavigateTo("/recipesoverview");
            }
        }

        protected void AddIngredient()
        {
            if (IngredientId != "0")
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
                }
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/recipesoverview");
        }

        private async void OnSelectionChanged(string value)
        {
            IngredientCategoryId = value;
            if (IngredientCategoryId != "0")
            {
                Ingredients = (await IngredientService.Search(int.Parse(IngredientCategoryId))).ToList();
            }
            StateHasChanged();
        }

        private void OnInputFileChange(InputFileChangeEventArgs e)
        {
            _selectedFiles = e.GetMultipleFiles();
            StateHasChanged();
        }
    }
}
