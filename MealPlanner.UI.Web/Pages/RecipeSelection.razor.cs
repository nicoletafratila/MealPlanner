using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Blazored.Modal;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class RecipeSelection : IComponent
    {
        [CascadingParameter]
        private BlazoredModalInstance BlazoredModal { get; set; } = default!;

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

        [Required]
        public string? RecipeId { get; set; }

        public PagedList<RecipeModel>? Recipes { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoryService { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var queryParameters = new QueryParameters()
            {
                Filters = new List<FilterItem>(),
                SortString = "DisplaySequence",
                SortDirection = SortDirection.Ascending,
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            Categories = await RecipeCategoryService!.SearchAsync(queryParameters);
            BlazoredModal.SetTitle("Select a recipe");
        }

        private async Task SaveAsync()
        {
            await BlazoredModal.CloseAsync(ModalResult.Ok(RecipeId));
        }

        private async Task CancelAsync()
        {
            await BlazoredModal.CancelAsync();
        }

        private async Task OnRecipeCategoryChangedAsync(string? value)
        {
            var filters = new List<FilterItem>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                filters.Add(new FilterItem("RecipeCategoryId", value, FilterOperator.Equals, StringComparison.OrdinalIgnoreCase));
            };
            var queryParameters = new QueryParameters()
            {
                Filters = filters,
                SortString = "Name",
                SortDirection = SortDirection.Ascending,
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            Recipes = await RecipeService!.SearchAsync(queryParameters);

            RecipeCategoryId = value;
            RecipeId = string.Empty;
            StateHasChanged();
        }
    }
}
