using BlazorBootstrap;
using Blazored.Modal.Services;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services.Http;
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

        [CascadingParameter(Name = "RefreshCurrentMealPlan")]
        private Func<Task>? RefreshCurrentMealPlan { get; set; }

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
                new() { Text = Resources.MealPlanEdit.BreadcrumbMealPlans, Href = "mealplans/mealplansoverview" },
                new() { Text = Resources.MealPlanEdit.BreadcrumbMealPlan, IsCurrentPage = true },
            ];

            var queryParameters = new QueryParameters<RecipeCategoryModel>
            {
                Filters = [],
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "DisplaySequence",
                        Direction = Common.Pagination.SortDirection.Ascending
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
                    Name = MealPlanService.GetMenuName(Resources.MealPlanEdit.MenuName),
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
                await ShowErrorAsync(Resources.MealPlanEdit.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.MealPlanEdit.SaveFailed);
                return;
            }

            if (RefreshCurrentMealPlan is not null)
                await RefreshCurrentMealPlan();
            await ShowInfoAsync(Resources.MealPlanEdit.SaveSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (MealPlan is null || MealPlan.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.MealPlanEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.MealPlanEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.MealPlanEdit.DeleteDialogTitle,
                message1: Resources.MealPlanEdit.DeleteDialogMessage1,
                message2: Resources.MealPlanEdit.DeleteDialogMessage2,
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
                await ShowErrorAsync(Resources.MealPlanEdit.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.MealPlanEdit.DeleteFailed);
                return;
            }

            if (RefreshCurrentMealPlan is not null)
                await RefreshCurrentMealPlan();
            await ShowInfoAsync(Resources.MealPlanEdit.DeleteSucceeded);
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
            {
                await ShowErrorAsync(Resources.MealPlanEdit.AddRecipeFailed);
                return;
            }

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
                YesButtonText = Resources.MealPlanEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.MealPlanEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.MealPlanEdit.DeleteDialogTitle,
                message1: Resources.MealPlanEdit.DeleteDialogMessage1,
                message2: Resources.MealPlanEdit.DeleteDialogMessage2,
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

            var modal = ModalService?.Show<ShopSelection>(Resources.MealPlanEdit.SelectShopModalTitle);
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
                await ShowErrorAsync(Resources.MealPlanEdit.ShoppingListSaveError);
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

            await _offCanvas.ShowAsync<RecipePreview>(title: Resources.MealPlanEdit.RecipeDetailsOffcanvasTitle, parameters: parameters);
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("mealplans/mealplansoverview");
        }

        private async Task OnRecipeCategoryChangedAsync(ChangeEventArgs e)
        {
            var recipeCategoryId = e.Value?.ToString();
            RecipeId = string.Empty;

            var filters = new List<Common.Pagination.FilterItem>();

            if (!string.IsNullOrWhiteSpace(recipeCategoryId))
            {
                filters.Add(new Common.Pagination.FilterItem(
                    "RecipeCategoryId",
                    recipeCategoryId,
                    Common.Pagination.FilterOperator.Equals,
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
                        Direction = Common.Pagination.SortDirection.Ascending
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

    }
}