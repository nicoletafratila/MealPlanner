using Common.Api;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class MealPlanEdit
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
                    OnRecipeCategoryChanged(_recipeCategoryId);
                }
            }
        }

        private string _recipeId;
        public string RecipeId
        {
            get
            {
                return _recipeId;
            }
            set
            {
                if (_recipeId != value)
                {
                    _recipeId = value;
                    OnRecipeChanged(_recipeId);
                }
            }
        }

        public EditMealPlanModel MealPlan { get; set; } = new EditMealPlanModel();
        public RecipeModel Recipe { get; set; } = new RecipeModel();
        public List<RecipeModel> Recipes { get; set; } = new List<RecipeModel>();
        public List<RecipeCategoryModel> Categories { get; set; } = new List<RecipeCategoryModel>();

        [Inject]
        public IMealPlanService MealPlanService { get; set; }

        [Inject]
        public IRecipeCategoryService RecipeCategoryService { get; set; }

        [Inject]
        public IRecipeService RecipeService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Categories = (await RecipeCategoryService.GetAll()).ToList();
            int.TryParse(Id, out var id);

            if (id == 0)
            {
                MealPlan = new EditMealPlanModel();
            }
            else
            {
                MealPlan = await MealPlanService.Get(int.Parse(Id));
            }
        }

        protected async Task Save()
        {
            if (MealPlan.Id == 0)
            {
                var addedEntity = await MealPlanService.Add(MealPlan);
                if (addedEntity != null)
                {
                    NavigateToOverview();
                }
            }
            else
            {
                await MealPlanService.Update(MealPlan);
                NavigateToOverview();
            }
        }

        protected async Task Delete()
        {
            if (MealPlan.Id != 0)
            {
                if (!await JSRuntime.Confirm($"Are you sure you want to delete the meal plan: '{MealPlan.Name}'?"))
                    return;

                await MealPlanService.DeleteAsync(MealPlan.Id);
                NavigateToOverview();
            }
        }

        protected bool CanAddRecipe
        {
            get
            {
                return !string.IsNullOrWhiteSpace(RecipeId) &&
                       RecipeId != "0";
            }
        }

        protected async Task AddRecipe()
        {
            if (!string.IsNullOrWhiteSpace(RecipeId) && RecipeId != "0")
            {
                RecipeModel item = MealPlan.Recipes.FirstOrDefault(i => i.Id == int.Parse(RecipeId));
                if (item == null)
                {
                    item = await RecipeService.Get(int.Parse(RecipeId));
                    MealPlan.Recipes.Add(item);
                }
            }
        }

        protected void EditRecipe(RecipeModel item)
        {
            NavigationManager.NavigateTo($"recipeedit/{item.Id}");
        }

        protected async Task DeleteRecipe(RecipeModel item)
        {
            RecipeModel itemToDelete = Recipes.FirstOrDefault(i => i.Id == item.Id);
            if (itemToDelete != null)
            {
                if (!await JSRuntime.Confirm($"Are you sure you want to delete the recipe '{itemToDelete.Name}'?"))
                    return;

                Recipes.Remove(itemToDelete);
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/mealplansoverview");
        }

        protected void ShowShoppingList()
        {
            NavigationManager.NavigateTo($"shoppinglist/{MealPlan.Id}");
        }

        private async void OnRecipeCategoryChanged(string value)
        {
            RecipeCategoryId = value;
            RecipeId = string.Empty;
            if (!string.IsNullOrWhiteSpace(RecipeCategoryId) && RecipeCategoryId != "0")
                Recipes = (await RecipeService.Search(int.Parse(RecipeCategoryId))).ToList();
            StateHasChanged();
        }

        private void OnRecipeChanged(string value)
        {
            RecipeId = value;
            StateHasChanged();
        }
    }
}
