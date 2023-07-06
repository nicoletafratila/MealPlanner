using AutoMapper;
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
                    OnRecipeCategoryChangedAsync(_recipeCategoryId!);
                }
            }
        }

        private string? _recipeId;
        public string? RecipeId
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
                    OnRecipeChanged(_recipeId!);
                }
            }
        }

        public EditMealPlanModel? MealPlan { get; set; }
        public RecipeModel? Recipe { get; set; }
        public IList<RecipeModel>? Recipes { get; set; }
        public IList<RecipeCategoryModel>? Categories { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoryService { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IShoppingListService? ShoppingListService { get; set; }

        [Inject]
        public IMapper? ShoppingListMapper { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Categories = await RecipeCategoryService!.GetAllAsync();

            if (id == 0)
            {
                MealPlan = new EditMealPlanModel();
            }
            else
            {
                MealPlan = await MealPlanService!.GetByIdAsync(int.Parse(Id!));
            }
        }

        protected async Task SaveAsync()
        {
            if (MealPlan!.Id == 0)
            {
                var addedEntity = await MealPlanService!.AddAsync(MealPlan);
                if (addedEntity != null)
                {
                    NavigateToOverview();
                }
            }
            else
            {
                await MealPlanService!.UpdateAsync(MealPlan);
                NavigateToOverview();
            }
        }

        protected async Task DeleteAsync()
        {
            if (MealPlan!.Id != 0)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the meal plan: '{MealPlan.Name}'?"))
                    return;

                await MealPlanService!.DeleteAsync(MealPlan.Id);
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

        protected async Task AddRecipeAsync()
        {
            if (!string.IsNullOrWhiteSpace(RecipeId) && RecipeId != "0")
            {
                RecipeModel? item = null;
                if (MealPlan != null)
                {
                    if (MealPlan.Recipes == null)
                    {
                        MealPlan.Recipes = new List<RecipeModel>();
                    }
                    item = MealPlan.Recipes.FirstOrDefault(i => i.Id == int.Parse(RecipeId));
                    if (item == null)
                    {
                        item = await RecipeService!.GetByIdAsync(int.Parse(RecipeId));
                        MealPlan.Recipes.Add(item!);
                    }
                }
            }
        }

        protected void EditRecipe(RecipeModel item)
        {
            NavigationManager!.NavigateTo($"recipeedit/{item.Id}");
        }

        protected async Task DeleteRecipeAsync(RecipeModel item)
        {
            RecipeModel? itemToDelete = MealPlan!.Recipes!.FirstOrDefault(i => i.Id == item.Id);
            if (itemToDelete != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the recipe '{itemToDelete.Name}'?"))
                    return;

                MealPlan.Recipes!.Remove(itemToDelete);
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/mealplansoverview");
        }

        protected async Task MakeShoppingListAsync()
        {
            var list = ShoppingListMapper!.Map<EditShoppingListModel>(MealPlan);
            var addedEntity = await ShoppingListService!.AddAsync(list);
            if (addedEntity != null)
            {
                NavigationManager!.NavigateTo($"shoppinglistedit/{addedEntity!.Id}");
            }
        }

        private async void OnRecipeCategoryChangedAsync(string value)
        {
            RecipeCategoryId = value;
            RecipeId = string.Empty;
            if (!string.IsNullOrWhiteSpace(RecipeCategoryId) && RecipeCategoryId != "0")
                Recipes = await RecipeService!.SearchAsync(int.Parse(RecipeCategoryId));
            StateHasChanged();
        }

        private void OnRecipeChanged(string value)
        {
            RecipeId = value;
            StateHasChanged();
        }
    }
}
