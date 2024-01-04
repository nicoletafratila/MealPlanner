using Blazored.Modal.Services;
using Common.Pagination;
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
        public EditMealPlanModel? MealPlan { get; set; }

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

        public string? RecipeId { get; set; }
        public RecipeModel? Recipe { get; set; }

        public PagedList<RecipeModel>? Recipes { get; set; }
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
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter]
        protected IModalService? Modal { get; set; } = default!;

        [CascadingParameter]
        protected IErrorComponent? ErrorComponent { get; set; }

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
                MealPlan = await MealPlanService!.GetEditAsync(int.Parse(Id!));
            }
        }

        private async Task SaveAsync()
        {
            var response = MealPlan!.Id == 0 ? await MealPlanService!.AddAsync(MealPlan) : await MealPlanService!.UpdateAsync(MealPlan);
            if (!string.IsNullOrWhiteSpace(response))
            {
                ErrorComponent!.ShowError("Error", response);
            }
            else
            {
                NavigateToOverview();
            }
        }

        private async Task DeleteAsync()
        {
            if (MealPlan!.Id != 0)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the meal plan: '{MealPlan.Name}'?"))
                    return;

                var response = await MealPlanService!.DeleteAsync(MealPlan.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError("Error", response);
                }
                else
                {
                    NavigateToOverview();
                }
            }
        }

        private bool CanAddRecipe
        {
            get
            {
                return !string.IsNullOrWhiteSpace(RecipeId) &&
                       RecipeId != "0";
            }
        }

        private async Task AddRecipeAsync()
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

        private void EditRecipe(RecipeModel item)
        {
            NavigationManager!.NavigateTo($"recipeedit/{item.Id}");
        }

        private async Task DeleteRecipeAsync(RecipeModel item)
        {
            RecipeModel? itemToDelete = MealPlan!.Recipes!.FirstOrDefault(i => i.Id == item.Id);
            if (itemToDelete != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the recipe '{itemToDelete.Name}'?"))
                    return;

                MealPlan.Recipes!.Remove(itemToDelete);
            }
        }

        private async Task SaveShoppingListAsync()
        {
            if (MealPlan is null || MealPlan.Recipes is null || !MealPlan.Recipes.Any())
                return;

            var shopSelectionModal = Modal.Show<ShopSelection>();
            var result = await shopSelectionModal.Result;

            if (result.Cancelled)
                return;

            if (result.Confirmed && result!.Data != null)
            {
                var shopId = result.Data.ToString();
                if (string.IsNullOrWhiteSpace(shopId))
                    return;

                var addedEntity = await ShoppingListService!.MakeShoppingListAsync(new MakeShoppingListModel { MealPlanId = MealPlan.Id, ShopId = int.Parse(shopId) });
                if (addedEntity != null && addedEntity!.Id > 0)
                {
                    NavigationManager!.NavigateTo($"shoppinglistedit/{addedEntity!.Id}");
                }
                else
                {
                    ErrorComponent!.ShowError("Error", "There has been an error when saving the shopping list");
                }
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/mealplansoverview");
        }

        private async void OnRecipeCategoryChangedAsync(string? value)
        {
            RecipeCategoryId = value;
            RecipeId = string.Empty;
            Recipes = await RecipeService!.SearchAsync(RecipeCategoryId);
            StateHasChanged();
        }
    }
}
