using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class IngredientEdit
    {
        [Parameter]
        public string Id { get; set; }

        private string _ingredientCategoryId;
        public string IngredientCategoryId
        {
            get {
                return _ingredientCategoryId;
            }
            set { 
                if (_ingredientCategoryId!= value)
                {
                    _ingredientCategoryId = value;
                    Model.IngredientCategoryId = int.Parse(_ingredientCategoryId);
                }
            }
        }

        private string _unitId;
        public string UnitId
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
                    Model.UnitId = int.Parse(_unitId);
                }
            }
        }

        public EditIngredientModel Model { get; set; } = new EditIngredientModel();
        public List<IngredientCategoryModel> Categories { get; set; } = new List<IngredientCategoryModel>();
        public List<UnitModel> Units { get; set; } = new List<UnitModel>();

        [Inject]
        public IIngredientService IngredientService { get; set; }

        [Inject]
        public IIngredientCategoryService CategoryService { get; set; }

        [Inject]
        public IUnitService UnitService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

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

        protected async Task NavigateToOverview()
        {
            NavigationManager.NavigateTo("/ingredientsoverview");
        }
    }
}
