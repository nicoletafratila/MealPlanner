using BlazorBootstrap;
using Blazored.Modal;
using Common.Pagination;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipeSelection
    {
        [CascadingParameter]
        public BlazoredModalInstance? ModalInstance { get; set; }

        public IModalController? ModalController { get; set; }

        public PagedList<RecipeCategoryModel>? Categories { get; private set; }

        public string? RecipeId { get; private set; }
        public PagedList<RecipeModel>? Recipes { get; private set; }

        [Inject]
        public IRecipeCategoryService RecipeCategoryService { get; set; } = default!;

        [Inject]
        public IRecipeService RecipeService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var queryParameters = new QueryParameters<RecipeCategoryModel>
            {
                Filters = [],
                Sorting = [],
                PageSize = int.MaxValue,
                PageNumber = 1
            };

            Categories = await RecipeCategoryService.SearchAsync(queryParameters)
                         ?? new PagedList<RecipeCategoryModel>([], new Metadata());
        }

        protected override void OnParametersSet()
        {
            if (ModalController is null && ModalInstance is not null)
            {
                ModalController = new BlazoredModalController(ModalInstance);
            }
        }

        private async Task SaveAsync()
        {
            await ModalController!.CloseAsync(RecipeId);
        }

        private async Task CancelAsync()
        {
            await ModalController!.CancelAsync();
        }

        private async Task OnRecipeCategoryChangedAsync(ChangeEventArgs e)
        {
            var recipeCategoryId = e.Value?.ToString();
            RecipeId = string.Empty;

            var filters = new List<FilterItem>();

            if (!string.IsNullOrWhiteSpace(recipeCategoryId))
            {
                filters.Add(new FilterItem(
                    propertyName: "RecipeCategoryId",
                    value: recipeCategoryId,
                    FilterOperator.Equals,
                    stringComparison: StringComparison.OrdinalIgnoreCase));
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
                PageSize = int.MaxValue,
                PageNumber = 1
            };

            Recipes = await RecipeService.SearchAsync(queryParameters) ?? new PagedList<RecipeModel>([], new Metadata());
            StateHasChanged();
        }
    }
}