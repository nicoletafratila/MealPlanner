using Common.UI;
using MealPlanner.Services;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class MainLayout : LayoutComponentBase, IMessageComponent
    {
        private MealPlanModel? _currentMealPlan;
        private bool _isAuthenticated;
        private CancellationTokenSource? _autoDismissCts;

        [Inject]
        public IMealPlanService MealPlanService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _currentMealPlan = await MealPlanService.GetCurrentAsync();
                _isAuthenticated = true;
            }
            catch
            {
            }
        }

        public bool IsErrorActive { get; private set; }
        public bool IsInfoActive { get; private set; }
        public bool IsWarningActive { get; private set; }
        public string? Message { get; private set; }

        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public Task ShowAsync(
            string message,
            MessageLevel level,
            string? title = null,
            Exception? exception = null,
            CancellationToken cancellationToken = default)
            => SetMessageAsync(message, level, cancellationToken);

        public Task ShowErrorAsync(
            string message,
            string? title = null,
            Exception? exception = null,
            CancellationToken cancellationToken = default)
            => ShowAsync(message, MessageLevel.Error, title, exception, cancellationToken);

        public Task ShowInfoAsync(
            string message,
            string? title = null,
            CancellationToken cancellationToken = default)
            => ShowAsync(message, MessageLevel.Info, title, null, cancellationToken);

        public Task ShowWarningAsync(
            string message,
            string? title = null,
            CancellationToken cancellationToken = default)
            => ShowAsync(message, MessageLevel.Warning, title, null, cancellationToken);

        private async Task SetMessageAsync(
            string message,
            MessageLevel level,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                await HideMessageAsync();
                return;
            }

            _autoDismissCts?.Cancel();
            _autoDismissCts?.Dispose();
            _autoDismissCts = null;

            IsErrorActive = level == MessageLevel.Error;
            IsWarningActive = level == MessageLevel.Warning;
            IsInfoActive = level == MessageLevel.Info;
            Message = message;

            await InvokeAsync(StateHasChanged);

            if (level == MessageLevel.Info)
            {
                _autoDismissCts = new CancellationTokenSource();
                var token = _autoDismissCts.Token;
                _ = Task.Delay(5000, token).ContinueWith(async t =>
                {
                    if (!t.IsCanceled)
                        await HideMessageAsync();
                }, TaskScheduler.Default);
            }
        }

        public Task HideMessageAsync()
        {
            _autoDismissCts?.Cancel();
            _autoDismissCts?.Dispose();
            _autoDismissCts = null;

            IsErrorActive = false;
            IsInfoActive = false;
            IsWarningActive = false;
            Message = null;

            return InvokeAsync(StateHasChanged);
        }

        public Task HideErrorAsync() => HideMessageAsync();

        public Task HideInfoAsync() => HideMessageAsync();
    }
}
