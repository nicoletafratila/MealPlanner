using System.ComponentModel.DataAnnotations;
using Blazored.Modal;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeSelection : IComponent
    {
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
        public IList<RecipeCategoryModel>? Categories { get; set; }

        [Required]
        public string? RecipeId { get; set; }

        public PagedList<RecipeModel>? Recipes { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoryService { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [CascadingParameter]
        protected BlazoredModalInstance BlazoredModal { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Categories = await RecipeCategoryService!.GetAllAsync();
            BlazoredModal.SetTitle("Select a recipe");
        }

        private async void SaveAsync()
        {
            await BlazoredModal.CloseAsync(ModalResult.Ok(RecipeId));
        }

        private async void CancelAsync()
        {
            await BlazoredModal.CancelAsync();
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
