using BlazorBootstrap;
using Blazored.Modal.Services;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class MealPlanEdit
    {
        private List<BreadcrumbItem>? navItems { get; set; }
        private ConfirmDialog dialog = default!;
        private Offcanvas offCanvas = default!;
        private GridTemplate<RecipeModel>? selectedRecipeGrid = default!;

        [CascadingParameter]
        private IModalService? modal { get; set; } = default!;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? messageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }
        public MealPlanEditModel? MealPlan { get; set; }

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
        public PagedList<RecipeCategoryModel>? Categories { get; set; }

        public string? RecipeId { get; set; }
        public PagedList<RecipeModel>? Recipes { get; set; }

        public RecipeModel? Recipe { get; set; }

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

        protected override async Task OnInitializedAsync()
        {
            navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Meal plans", Href ="/mealplansoverview" },
                new BreadcrumbItem{ Text = "Meal plan", IsCurrentPage = true },
            };

            var queryParameters = new QueryParameters()
            {
                Filters = new List<FilterItem>(),
                SortString = "DisplaySequence",
                SortDirection = SortDirection.Ascending,
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            Categories = await RecipeCategoryService!.SearchAsync(queryParameters);

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                MealPlan = new MealPlanEditModel();
            }
            else
            {
                MealPlan = await MealPlanService!.GetEditAsync(id);
            }
        }

        private async Task SaveAsync()
        {
            var response = MealPlan?.Id == 0 ? await MealPlanService!.AddAsync(MealPlan) : await MealPlanService!.UpdateAsync(MealPlan!);
            if (response != null && !response.Succeeded)
            {
                messageComponent?.ShowError(response.Message!);
            }
            else
            {
                messageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async Task DeleteAsync()
        {
            if (MealPlan?.Id != 0)
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

                var response = await MealPlanService!.DeleteAsync(MealPlan!.Id);
                if (response != null && !response.Succeeded)
                {
                    messageComponent?.ShowError(response.Message!);
                }
                else
                {
                    messageComponent?.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private async Task<GridDataProviderResult<RecipeModel>> RecipesDataProviderAsync(GridDataProviderRequest<RecipeModel> request)
        {
            var data = MealPlan!.Recipes == null ? new List<RecipeModel>() : MealPlan.Recipes;
            return await Task.FromResult(new GridDataProviderResult<RecipeModel> { Data = data, TotalCount = data.Count });
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
                        MealPlan.Recipes.SetIndexes();
                        await selectedRecipeGrid!.RefreshDataAsync();
                    }
                }

                StateHasChanged();
            }
        }

        private async Task DeleteRecipeAsync(RecipeModel item)
        {
            RecipeModel? itemToDelete = MealPlan?.Recipes?.FirstOrDefault(i => i.Id == item.Id);
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

                MealPlan?.Recipes?.Remove(itemToDelete);
                MealPlan?.Recipes?.SetIndexes();
                await selectedRecipeGrid!.RefreshDataAsync();
                StateHasChanged();
            }
        }

        private async Task SaveShoppingListAsync()
        {
            if (MealPlan is null || MealPlan.Recipes is null || !MealPlan.Recipes.Any())
                return;

            var shopSelectionModal = modal?.Show<ShopSelection>();
            var result = await shopSelectionModal!.Result;

            if (result.Cancelled)
                return;

            if (result.Confirmed && result?.Data != null)
            {
                if (!int.TryParse(result.Data.ToString(), out int shopId))
                    return;

                var addedEntity = await ShoppingListService!.MakeShoppingListAsync(new ShoppingListCreateModel { MealPlanId = MealPlan.Id, ShopId = shopId });
                if (addedEntity != null && addedEntity?.Id > 0)
                {
                    NavigationManager?.NavigateTo($"shoppinglistedit/{addedEntity?.Id}");
                }
                else
                {
                    messageComponent?.ShowError("There has been an error when saving the shopping list");
                }
            }
        }

        private async Task ShowRecipeAsync(RecipeModel item)
        {
            var recipe = await RecipeService!.GetEditAsync(item.Id);
            var parameters = new Dictionary<string, object>
            {
                { "Recipe", recipe! },
                { "RecipeCategory", item.RecipeCategory?.Name! },
            };
            await offCanvas.ShowAsync<RecipePreview>(title: "Recipe details", parameters: parameters);
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo("/mealplansoverview");
        }

        private async Task OnRecipeCategoryChangedAsync(string? value)
        {
            RecipeCategoryId = value;
            RecipeId = string.Empty;

            var filters = new List<FilterItem>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                filters.Add(new FilterItem("RecipeCategoryId", value, FilterOperator.Equals, StringComparison.OrdinalIgnoreCase));
            }
            ;
            var queryParameters = new QueryParameters()
            {
                Filters = filters,
                SortString = "Name",
                SortDirection = SortDirection.Ascending,
                PageNumber = 1,
                PageSize = int.MaxValue,
            };
            Recipes = await RecipeService!.SearchAsync(queryParameters);
            StateHasChanged();
        }
    }
}
