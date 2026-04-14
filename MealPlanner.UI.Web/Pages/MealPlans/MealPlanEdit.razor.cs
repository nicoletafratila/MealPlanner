using BlazorBootstrap;
using Blazored.Modal.Services;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class MealPlanEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private Offcanvas _offCanvas = default!;
        private Shared.GridTemplate<RecipeModel>? _selectedRecipeGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyHorizontalClass;

        [CascadingParameter]
        private IModalService? ModalService { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public MealPlanEditModel MealPlan { get; set; } = new();

        public PagedList<RecipeCategoryModel>? Categories { get; set; }

        public string? RecipeId { get; set; }
        public PagedList<RecipeModel>? Recipes { get; set; }

        public RecipeModel? Recipe { get; set; }

        [Inject]
        public IMealPlanService MealPlanService { get; set; } = default!;

        [Inject]
        public IRecipeCategoryService RecipeCategoryService { get; set; } = default!;

        [Inject]
        public IRecipeService RecipeService { get; set; } = default!;

        [Inject]
        public IShoppingListService ShoppingListService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new() { Text = "Meal plans", Href = "mealplans/mealplansoverview" },
                new() { Text = "Meal plan", IsCurrentPage = true },
            ];

            var queryParameters = new QueryParameters<RecipeCategoryModel>
            {
                Filters = [],
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "DisplaySequence",
                        Direction = SortDirection.Ascending
                    }
                ],
                PageSize = int.MaxValue,
                PageNumber = 1
            };

            Categories = await RecipeCategoryService.SearchAsync(queryParameters);

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                MealPlan = new MealPlanEditModel
                {
                    Recipes = [],
                    Name = GetMenuName(),
                };
            }
            else
            {
                MealPlan = await MealPlanService.GetEditAsync(id) ?? new MealPlanEditModel { Id = id, Recipes = [] };
                MealPlan.Recipes ??= [];
            }

            _selectedRecipeGrid = new Shared.GridTemplate<RecipeModel>();
        }

        private async Task SaveAsync()
        {
            if (MealPlan is null)
                return;

            await SaveCoreAsync(MealPlan);
        }

        private async Task SaveCoreAsync(MealPlanEditModel mealPlan)
        {
            CommandResponse? response;

            if (mealPlan.Id == 0)
            {
                response = await MealPlanService.AddAsync(mealPlan);
            }
            else
            {
                response = await MealPlanService.UpdateAsync(mealPlan);
            }

            if (response is null)
            {
                await ShowErrorAsync("Save failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? "Save failed.");
                return;
            }

            await ShowInfoAsync("Data has been saved successfully");
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (MealPlan is null || MealPlan.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = "OK",
                YesButtonColor = ButtonColor.Success,
                NoButtonText = "Cancel",
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: "Are you sure you want to delete this?",
                message1: "This will delete the record. Once deleted can not be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(MealPlan);
        }

        private async Task DeleteCoreAsync(MealPlanEditModel mealPlan)
        {
            if (mealPlan.Id == 0)
                return;

            var response = await MealPlanService.DeleteAsync(mealPlan.Id);
            if (response is null)
            {
                await ShowErrorAsync("Delete failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? "Delete failed.");
                return;
            }

            await ShowInfoAsync("Data has been deleted successfully");
            NavigateToOverview();
        }

        private async Task<GridDataProviderResult<RecipeModel>> RecipesDataProviderAsync(
            GridDataProviderRequest<RecipeModel> request)
        {
            var data = MealPlan.Recipes ?? [];

            _tableGridClass = data.Count == 0
                ? CssClasses.GridTemplateEmptyHorizontalClass
                : CssClasses.GridTemplateWithItemsHorizontalClass;

            StateHasChanged();

            return await Task.FromResult(new GridDataProviderResult<RecipeModel>
            {
                Data = data,
                TotalCount = data.Count
            });
        }

        private bool CanAddRecipe =>
            !string.IsNullOrWhiteSpace(RecipeId) &&
            RecipeId != "0";

        private async Task AddRecipeAsync()
        {
            if (!CanAddRecipe || MealPlan is null)
                return;

            MealPlan.Recipes ??= [];

            var recipeId = int.Parse(RecipeId!);
            var existing = MealPlan.Recipes.FirstOrDefault(r => r.Id == recipeId);
            if (existing is not null)
            {
                StateHasChanged();
                return;
            }

            var recipe = await RecipeService.GetByIdAsync(recipeId);
            if (recipe is null)
                return;

            MealPlan.Recipes.Add(recipe);
            MealPlan.Recipes.SetIndexes();

            if (_selectedRecipeGrid is not null)
                await _selectedRecipeGrid.RefreshDataAsync();

            StateHasChanged();
        }

        private async Task DeleteRecipeAsync(RecipeModel item)
        {
            if (MealPlan?.Recipes is null)
                return;

            var itemToDelete = MealPlan.Recipes.FirstOrDefault(r => r.Id == item.Id);
            if (itemToDelete is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = "OK",
                YesButtonColor = ButtonColor.Success,
                NoButtonText = "Cancel",
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: "Are you sure you want to delete this?",
                message1: "This will delete the record. Once deleted can not be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            MealPlan.Recipes.Remove(itemToDelete);
            MealPlan.Recipes.SetIndexes();

            if (_selectedRecipeGrid is not null)
                await _selectedRecipeGrid.RefreshDataAsync();

            StateHasChanged();
        }

        private async Task SaveShoppingListAsync()
        {
            if (MealPlan is null || MealPlan.Recipes is null || !MealPlan.Recipes.Any())
                return;

            var modal = ModalService?.Show<ShopSelection>("Select a shop");
            if (modal is null)
                return;

            var result = await modal.Result;

            if (result.Cancelled || !result.Confirmed || result.Data is null)
                return;

            if (!int.TryParse(result.Data.ToString(), out var shopId))
                return;

            var addedEntity = await ShoppingListService.MakeShoppingListAsync(
                new ShoppingListCreateModel { MealPlanId = MealPlan.Id, ShopId = shopId });

            if (addedEntity is not null && addedEntity.Id > 0)
            {
                NavigationManager.NavigateTo($"mealplans/shoppinglistedit/{addedEntity.Id}");
            }
            else
            {
                await ShowErrorAsync("There has been an error when saving the shopping list");
            }
        }

        private async Task ShowRecipeAsync(RecipeModel item)
        {
            var recipe = await RecipeService.GetEditAsync(item.Id);
            if (recipe is null)
                return;

            var parameters = new Dictionary<string, object>
            {
                { "Recipe", recipe },
                { "RecipeCategory", item.RecipeCategory?.Name ?? string.Empty },
            };

            await _offCanvas.ShowAsync<RecipePreview>(title: "Recipe details", parameters: parameters);
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("mealplans/mealplansoverview");
        }

        private async Task OnRecipeCategoryChangedAsync(ChangeEventArgs e)
        {
            var recipeCategoryId = e.Value?.ToString();
            RecipeId = string.Empty;

            var filters = new List<FilterItem>();

            if (!string.IsNullOrWhiteSpace(recipeCategoryId))
            {
                filters.Add(new FilterItem(
                    "RecipeCategoryId",
                    recipeCategoryId,
                    FilterOperator.Equals,
                    StringComparison.OrdinalIgnoreCase));
            }

            var queryParameters = new QueryParameters<RecipeModel>
            {
                Filters = filters,
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "Name",
                        Direction = SortDirection.Ascending
                    }
                ],
                PageNumber = 1,
                PageSize = int.MaxValue
            };

            Recipes = await RecipeService.SearchAsync(queryParameters);
            StateHasChanged();
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);

        private static string GetMenuName()
        {
            var now = DateTime.Now;
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            int week = calendar.GetWeekOfYear(now, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return $"{Resources.MealPlansOverview.MenuName} {now.Year}/{week}";
        }
    }
}