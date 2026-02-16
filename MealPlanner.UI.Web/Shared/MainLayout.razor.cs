using Common.UI;

namespace MealPlanner.UI.Web.Shared
{
    public partial class MainLayout : IMessageComponent
    {
        public bool IsErrorActive { get; private set; }
        public bool IsInfoActive { get; private set; }
        public string? Message { get; private set; }

        public void ShowError(string message) => SetMessage(message, isError: true);

        public void ShowInfo(string message) => SetMessage(message, isError: false);

        private void SetMessage(string message, bool isError)
        {
            IsErrorActive = isError;
            IsInfoActive = !isError;
            Message = message;
            StateHasChanged();
        }

        protected void HideError()
        {
            IsErrorActive = false;
            StateHasChanged();
        }

        protected void HideInfo()
        {
            IsInfoActive = false;
            StateHasChanged();
        }
    }
}
