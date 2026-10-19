using Microsoft.AspNetCore.SignalR;
using RecipeBook.Api.Hubs;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Services
{
    public class RecipeShareProgressNotifier(IHubContext<RecipeShareProgressHub> hubContext) : IRecipeShareProgressNotifier
    {
        private const string ProgressMessage = "ShareProgress";

        private readonly IHubContext<RecipeShareProgressHub> _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

        public async Task NotifyAsync(Job job, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(job);

            var status = new JobStatusModel
            {
                JobId = job.Id,
                Status = (Shared.Enums.JobStatus)(int)job.Status,
                ProcessedCount = job.ProcessedCount,
                TotalCount = job.TotalCount,
                PercentComplete = job.TotalCount <= 0 ? 0 : (int)(job.ProcessedCount * 100.0 / job.TotalCount),
                ErrorMessage = job.ErrorMessage
            };

            await _hubContext.Clients.Group(job.Id.ToString()).SendAsync(ProgressMessage, status, cancellationToken);
        }
    }
}
