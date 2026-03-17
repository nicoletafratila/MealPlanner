namespace Common.UI
{
    public interface IMessageComponent
    {
        Task ShowAsync(
            string message,
            MessageLevel level,
            string? title = null,
            Exception? exception = null,
            CancellationToken cancellationToken = default);

        Task ShowErrorAsync(string message, string? title = null, Exception? exception = null,
            CancellationToken cancellationToken = default);

        Task ShowInfoAsync(string message, string? title = null,
            CancellationToken cancellationToken = default);

        Task ShowWarningAsync(string message, string? title = null,
            CancellationToken cancellationToken = default);
    }
}
