using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeEdit
    {
        [Inject]
        public IRecipeService RecipeService { get; set; }

        [Inject]
        public IRecipeCategoryService CategoryService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Id { get; set; }
        protected string RecipeCategoryId = string.Empty;

        public EditRecipeModel Model { get; set; } = new EditRecipeModel();
        public List<RecipeCategoryModel> Categories { get; set; } = new List<RecipeCategoryModel>();
        private IReadOnlyList<IBrowserFile> _selectedFiles;

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Categories = (await CategoryService.GetAll()).ToList();

            if (id == 0)
            {
                Model = new EditRecipeModel();
            }
            else
            {
                Model = await RecipeService.Get(int.Parse(Id));
            }

            RecipeCategoryId = Model.RecipeCategoryId.ToString();
        }

        protected async Task Save()
        {
            Model.RecipeCategoryId = int.Parse(RecipeCategoryId);
            if (_selectedFiles != null)
            {
                var file = _selectedFiles[0];
                Stream stream = file.OpenReadStream();
                MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                stream.Close();
                Model.ImageContent = ms.ToArray();
            }

            if (Model.Id == 0)
            {
                var addedEntity = await RecipeService.Add(Model);
                if (addedEntity != null)
                {
                    NavigationManager.NavigateTo("/recipesoverview");
                }
            }
            else
            {
                await RecipeService.Update(Model);
                NavigationManager.NavigateTo("/recipesoverview");
            }
        }

        private void OnInputFileChange(InputFileChangeEventArgs e)
        {
            _selectedFiles = e.GetMultipleFiles();
            StateHasChanged();
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/recipesoverview");
        }
    }
}
