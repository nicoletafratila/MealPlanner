using Common.Logging;
using Common.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class AuditTrail
    {
        private IEnumerable<LogModel>? Logs;

        [Inject]
        public ILoggerService? LoggerService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Logs = await LoggerService!.GetLogsAsync();
        }

        private async void DeleteAllLogs()
        {
            await LoggerService!.DeleteLogsAsync();
            await OnInitializedAsync();
            StateHasChanged();
        }
    }
}
