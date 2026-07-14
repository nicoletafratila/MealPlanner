using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MealPlanner.UI.Mobile.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        protected static readonly List<SortingModel> DefaultSorting =
            [new SortingModel { PropertyName = "Name", Direction = SortDirection.Ascending }];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private string? _successMessage;

        public bool IsNotBusy => !IsBusy;

        protected void SetError(string? message)
        {
            ErrorMessage = message;
            SuccessMessage = null;
        }

        protected void SetSuccess(string? message)
        {
            SuccessMessage = message;
            ErrorMessage = null;
        }

        protected void ClearMessages()
        {
            ErrorMessage = null;
            SuccessMessage = null;
        }
    }
}
