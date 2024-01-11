using MealPlanner.UI.Web.Services;

namespace MealPlanner.UI.Web.Shared
{
    public partial class MainLayout : IErrorComponent
    {
        public bool IsErrorActive { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }

        public void ShowError(string title, string message)
        {
            IsErrorActive = true;
            Title = title;
            Message = message;
            StateHasChanged();
        }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected void HideError()
        {
            IsErrorActive = false;
        }
    }
}
