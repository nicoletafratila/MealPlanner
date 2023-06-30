using Common.Api;
using Common.Data.Entities;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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
        
        public MarkupString AlertMessage { get; set; }
        public string? AlertClass { get; set; }
        private long maxFileSize = 1024L * 1024L * 1024L * 3L;

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

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            try
            {
                if (e.File != null)
                {
                    Stream stream = e.File.OpenReadStream(maxAllowedSize: 1024 * 300);
                    MemoryStream ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    stream.Close();
                    Ingredient!.ImageContent = ms.ToArray();
                    SetAlert();
                }
                StateHasChanged();
            }
            catch (Exception)
            {
                SetAlert("alert alert-danger", "oi oi-ban", $"File size exceeds the limit. Maximum allowed size is <strong>{maxFileSize / (1024 * 1024)} MB</strong>.");
                return;
            }
        }

        private void SetAlert(string alertClass = "", string iconClass = "", string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                AlertMessage = new MarkupString();
                AlertClass = string.Empty;
            }
            {
                AlertMessage = new MarkupString($"<span class='{iconClass}' aria-hidden='true'></span> {message}");
                AlertClass = alertClass;
            }
        }
    }
}
