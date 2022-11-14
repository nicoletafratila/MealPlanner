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
        protected string CategoryId = string.Empty;

        public EditRecipeModel Model { get; set; } = new EditRecipeModel();
        public List<RecipeCategoryModel> Categories { get; set; } = new List<RecipeCategoryModel>();
        private IReadOnlyList<IBrowserFile> _selectedFiles;
        protected bool Saved;

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Categories = (await CategoryService.GetAll()).ToList();
            Saved = false;

            if (id == 0)
            {
                Model = new EditRecipeModel();
            }
            else
            {
                Model = await RecipeService.Get(int.Parse(Id));
            }

            CategoryId = Model.CategoryId.ToString();
        }

        protected async Task Save()
        {
            Saved = false;
            Model.CategoryId = int.Parse(CategoryId);
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
                    Saved = true;
                }
                else
                {
                    Saved = false;
                }
            }
            else
            {
                await RecipeService.Update(Model);
                Saved = true;
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
