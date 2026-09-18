using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MealPlanner.UI.Mobile.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        private const int DefaultSearchDebounceMilliseconds = 400;

        protected static readonly List<SortingModel> DefaultSorting =
            [new SortingModel { PropertyName = "Name", Direction = SortDirection.Ascending }];

        private CancellationTokenSource? _searchDebounceTokenSource;

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

        protected void DebounceSearch(IRelayCommand searchCommand, string? value, int debounceMilliseconds = DefaultSearchDebounceMilliseconds)
        {
            _searchDebounceTokenSource?.Cancel();

            if (string.IsNullOrEmpty(value))
            {
                searchCommand.Execute(null);
                return;
            }

            var tokenSource = new CancellationTokenSource();
            _searchDebounceTokenSource = tokenSource;
            DebounceSearchAsync(searchCommand, tokenSource.Token, debounceMilliseconds);
        }

        private static async void DebounceSearchAsync(IRelayCommand searchCommand, CancellationToken cancellationToken, int debounceMilliseconds)
        {
            try
            {
                await Task.Delay(debounceMilliseconds, cancellationToken);
                searchCommand.Execute(null);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
