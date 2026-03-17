using Common.UI;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class MainLayout : LayoutComponentBase, IMessageComponent
    {
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

            IsErrorActive = level == MessageLevel.Error;
            IsWarningActive = level == MessageLevel.Warning;
            IsInfoActive = level == MessageLevel.Info;
            Message = message;

            await InvokeAsync(StateHasChanged);
        }

        public Task HideMessageAsync()
        {
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
