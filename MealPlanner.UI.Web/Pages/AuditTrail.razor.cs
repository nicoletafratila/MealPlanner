using Common.Logging;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize(Roles = "admin")]
    public partial class AuditTrail
    {
        public IEnumerable<LogModel>? Logs;

        [Inject]
        public ILoggerService LoggerService { get; set; } = default!;

        [Inject]
        public ILogger<AuditTrail> Logger { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            try
            {
                Logs = await LoggerService.GetLogsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load audit trail logs.");
                Logs = [];
            }
        }

        private async Task DeleteAllLogsAsync()
        {
            try
            {
                await LoggerService.DeleteLogsAsync();
                await LoadLogsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to delete audit trail logs.");
            }
            finally
            {
                StateHasChanged();
            }
        }
    }
}
