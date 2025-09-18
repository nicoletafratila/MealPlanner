using BlazorBootstrap;
using Common.Constants;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class RecipesOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem>? _navItems = default!;
        private GridTemplate<RecipeModel>? _recipesGrid = default!;
        private string _tableGridClass = CssClasses.GridTemplateNoRowsClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="/" }
            };
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"recipeedit/");
        }

        private void Update(RecipeModel item)
        {
            NavigationManager?.NavigateTo($"recipeedit/{item.Id}");
        }

        private async Task DeleteAsync(RecipeModel item)
        {
            if (item != null)
            {
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

                var response = await RecipeService!.DeleteAsync(item.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await _recipesGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<RecipeModel>
            {
                Filters = new List<FilterItem>() { },
                Sorting = new List<SortingItem<RecipeModel>>
                        {
                            new SortingItem<RecipeModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                PageNumber = 1,
                PageSize = 10
            };
            await RecipesDataProviderAsync(request);
        }

        private async Task<GridDataProviderResult<RecipeModel>> RecipesDataProviderAsync(GridDataProviderRequest<RecipeModel> request)
        {
            string sortString = "";
            SortDirection sortDirection = SortDirection.None;

            if (request.Sorting is not null && request.Sorting.Any())
            {
                sortString = request.Sorting.FirstOrDefault()!.SortString;
                sortDirection = request.Sorting.FirstOrDefault()!.SortDirection;
            }
            var queryParameters = new QueryParameters()
            {
                Filters = request.Filters,
                SortString = sortString,
                SortDirection = sortDirection,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await RecipeService!.SearchAsync(queryParameters);
            if (result == null || result.Items == null)
            {
                result = new PagedList<RecipeModel>(new List<RecipeModel>(), new Metadata());
            }
            return await Task.FromResult(new GridDataProviderResult<RecipeModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount });
        }
    }
}
