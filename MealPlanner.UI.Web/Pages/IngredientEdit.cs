using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class IngredientEdit
    {
        [Inject]
        public IIngredientService IngredientService { get; set; }

        [Inject]
        public IIngredientCategoryService CategoryService { get; set; }

        [Inject]
        public IUnitService UnitService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Id { get; set; }
        protected string IngredientCategoryId = string.Empty;
        protected string UnitId = string.Empty;

        public EditIngredientModel Model { get; set; } = new EditIngredientModel();
        public List<IngredientCategoryModel> Categories { get; set; } = new List<IngredientCategoryModel>();
        public List<UnitModel> Units { get; set; } = new List<UnitModel>();

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Categories = (await CategoryService.GetAll()).ToList();
            Units = (await UnitService.GetAll()).ToList();

            if (id == 0)
            {
                Model = new EditIngredientModel();
            }
            else
            {
                Model = await IngredientService.Get(int.Parse(Id));
            }

            IngredientCategoryId = Model.IngredientCategoryId.ToString();
            UnitId = Model.UnitId.ToString();
        }

        protected async Task Save()
        {
            Model.IngredientCategoryId = int.Parse(IngredientCategoryId);
            Model.UnitId = int.Parse(UnitId);

            if (Model.Id == 0)
            {
                var addedEntity = await IngredientService.Add(Model);
                if (addedEntity != null)
                {
                    NavigationManager.NavigateTo("/ingredientsoverview");
                }
            }
            else
            {
                await IngredientService.Update(Model);
                NavigationManager.NavigateTo("/ingredientsoverview");
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/ingredientsoverview");
        }
    }
}
