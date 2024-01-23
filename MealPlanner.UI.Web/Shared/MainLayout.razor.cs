using MealPlanner.UI.Web.Services;

namespace MealPlanner.UI.Web.Shared
{
    public partial class MainLayout : IMessageComponent
    {
        public bool IsErrorActive { get; set; }
        public bool IsInfoActive { get; set; }
        public string? Message { get; set; }

        public void ShowError(string message)
        {
            IsErrorActive = true;
            Message = message;
            StateHasChanged();
        }

        public void ShowInfo(string message)
        {
            IsInfoActive = true;
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

        protected void HideInfo()
        {
            IsInfoActive = false;
        }
    }
}
