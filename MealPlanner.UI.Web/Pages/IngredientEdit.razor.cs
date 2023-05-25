using Common.Api;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class IngredientEdit
    {
        [Parameter]
        public string? Id { get; set; }

        private string? _ingredientCategoryId;
        public string? IngredientCategoryId
        {
            get
            {
                return _ingredientCategoryId;
            }
            set
            {
                if (_ingredientCategoryId != value)
                {
                    _ingredientCategoryId = value;
                    Ingredient!.IngredientCategoryId = int.Parse(_ingredientCategoryId!);
                }
            }
        }

        private string? _unitId;
        public string? UnitId
        {
            get
            {
                return _unitId;
            }
            set
            {
                if (_unitId != value)
                {
                    _unitId = value;
                    Ingredient!.UnitId = int.Parse(_unitId!);
                }
            }
        }

        public EditIngredientModel? Ingredient { get; set; }
        public IList<IngredientCategoryModel>? Categories { get; set; }
        public IList<UnitModel>? Units { get; set; }

        [Inject]
        public IIngredientService? IngredientService { get; set; }

        [Inject]
        public IIngredientCategoryService? CategoryService { get; set; }

        [Inject]
        public IUnitService? UnitService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Categories = await CategoryService!.GetAllAsync();
            Units = await UnitService!.GetAllAsync();

            if (id == 0)
            {
                Ingredient = new EditIngredientModel();
            }
            else
            {
                Ingredient = await IngredientService!.GetByIdAsync(int.Parse(Id!));
            }

            IngredientCategoryId = Ingredient!.IngredientCategoryId.ToString();
            UnitId = Ingredient.UnitId.ToString();
        }

        protected async Task SaveAsync()
        {
            if (Ingredient!.Id == 0)
            {
                var addedEntity = await IngredientService!.AddAsync(Ingredient);
                if (addedEntity != null)
                {
                    NavigateToOverview();
                }
            }
            else
            {
                await IngredientService!.UpdateAsync(Ingredient);
                NavigateToOverview();
            }
        }

        protected async Task DeleteAsync()
        {
            if (Ingredient!.Id != 0)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the ingredient: '{Ingredient.Name}'?"))
                    return;

                await IngredientService!.DeleteAsync(Ingredient.Id);
                NavigateToOverview();
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/ingredientsoverview");
        }
    }
}
