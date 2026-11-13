using System.Text.Json;
using Common.Pagination;
using Common.Services;
using MediatR;
using Newtonsoft.Json;
using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;
using RecipeBook.Data.Entities.Enums;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.ShareAll
{
    public class StartShareAllCommandHandler(
        IJobRepository jobRepository,
        IJobQueue jobQueue,
        ICurrentUserService currentUserService,
        ILogger<StartShareAllCommandHandler> logger) : IRequestHandler<StartShareAllCommand, JobStartedModel?>
    {
        private readonly IJobRepository _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        private readonly IJobQueue _jobQueue = jobQueue ?? throw new ArgumentNullException(nameof(jobQueue));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<StartShareAllCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<JobStartedModel?> Handle(StartShareAllCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return new JobStartedModel { Succeeded = false, Message = RecipeMessages.UserIdRequired };

                var job = new Data.Entities.Job
                {
                    OwnerUserId = userId,
                    TargetUserId = request.TargetUserId,
                    FiltersJson = request.Filters is null ? null : JsonConvert.SerializeObject(NormalizeFilters(request.Filters)),
                    Status = JobStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                job = await _jobRepository.AddAsync(job, cancellationToken);
                _jobQueue.Enqueue(job.Id);

                return new JobStartedModel { Succeeded = true, JobId = job.Id };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when starting a share-all job to user {TargetUserId}.", request.TargetUserId);
                return new JobStartedModel { Succeeded = false, Message = RecipeMessages.SaveFailed };
            }
        }

        /// <summary>
        /// ASP.NET model-binds <see cref="FilterItem.Value"/> using System.Text.Json, so it arrives here as a boxed
        /// <see cref="JsonElement"/>. Newtonsoft (used to persist <see cref="Data.Entities.Job.FiltersJson"/>) has no
        /// special handling for that type and would serialize its internal struct fields instead of the scalar
        /// value, corrupting the filter once the job re-deserializes it. Unwrap to a plain CLR value first.
        /// </summary>
        private static IEnumerable<FilterItem> NormalizeFilters(IEnumerable<FilterItem> filters) =>
            filters.Select(f => new FilterItem(f.PropertyName, NormalizeValue(f.Value), f.Operator, f.StringComparison));

        private static object? NormalizeValue(object? value) =>
            value is JsonElement element
                ? element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.TryGetInt64(out var longValue) ? longValue : element.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => throw new NotSupportedException($"Unsupported filter value kind {element.ValueKind}.")
                }
                : value;
    }
}
