using MealPlanner.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RecipeBook.Shared.Models;

namespace MealPlanner.App.Pages
{
    public partial class RecipeEdit
    {
        [Inject]
        public IRecipeService RecipeDataService { get; set; }

        [Parameter]
        public string Id { get; set; }

        public RecipeModel Model { get; set; } = new RecipeModel();

        protected bool Saved;
        private IReadOnlyList<IBrowserFile> _selectedFiles;

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Saved = false;

            if (id == 0)
            {
                Model = new RecipeModel();
            }
            else
            {
                Model = await RecipeDataService.Get(int.Parse(Id));
            }
        }

        protected async Task Save()
        {
            Saved = false;

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
                var addedEntity = await RecipeDataService.Add(Model);
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
                await RecipeDataService.Update(Model);
                Saved = true;
            }
        }

        private void OnInputFileChange(InputFileChangeEventArgs e)
        {
            _selectedFiles = e.GetMultipleFiles();
            StateHasChanged();
        }
    }
}
