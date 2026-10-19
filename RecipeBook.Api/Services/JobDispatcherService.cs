using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Api.Services
{
    /// <summary>
    /// Hosted service that drains queued share-all job ids and runs them through <see cref="IRecipeJobProcessor"/>,
    /// one job at a time, in a fresh DI scope per job. On startup it reconciles state left behind by a prior crash:
    /// any job still "Running" couldn't have survived the restart, so it's marked Failed; any "Pending" job is re-queued.
    /// </summary>
    public class JobDispatcherService(
        IServiceScopeFactory scopeFactory,
        IJobQueue queue,
        ILogger<JobDispatcherService> logger) : BackgroundService
    {
        private static readonly TimeSpan PollingFallback = TimeSpan.FromMinutes(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ReconcileAsync(stoppingToken);

            using var timer = new PeriodicTimer(PollingFallback);
            _ = PollPendingPeriodicallyAsync(timer, stoppingToken);

            await foreach (var jobId in queue.ReadAllAsync(stoppingToken))
                await ProcessJobSafelyAsync(jobId, stoppingToken);
        }

        private async Task ReconcileAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var notifier = scope.ServiceProvider.GetRequiredService<IRecipeShareProgressNotifier>();

            var orphaned = await jobRepository.GetOrphanedRunningAsync(cancellationToken);
            foreach (var job in orphaned)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = RecipeMessages.JobInterruptedByRestart;
                job.CompletedAt = DateTime.UtcNow;
                await jobRepository.UpdateAsync(job, cancellationToken);
                await notifier.NotifyAsync(job, cancellationToken);
            }

            var pending = await jobRepository.GetPendingAsync(cancellationToken);
            foreach (var job in pending)
                queue.Enqueue(job.Id);
        }

        private async Task PollPendingPeriodicallyAsync(PeriodicTimer timer, CancellationToken cancellationToken)
        {
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken))
                {
                    using var scope = scopeFactory.CreateScope();
                    var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                    var pending = await jobRepository.GetPendingAsync(cancellationToken);
                    foreach (var job in pending)
                        queue.Enqueue(job.Id);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ProcessJobSafelyAsync(Guid jobId, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IRecipeJobProcessor>();
                await processor.ProcessAsync(jobId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error while processing share job {JobId}.", jobId);
            }
        }
    }
}
