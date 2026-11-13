using Common.Services;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Job.Queries.GetStatus
{
    public class GetStatusQueryHandler(
        IJobRepository jobRepository,
        ICurrentUserService currentUserService) : IRequestHandler<GetStatusQuery, JobStatusModel?>
    {
        private readonly IJobRepository _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        public async Task<JobStatusModel?> Handle(GetStatusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
            if (job is null || job.OwnerUserId != _currentUserService.UserId)
                return null;

            return new JobStatusModel
            {
                JobId = job.Id,
                Status = (Shared.Enums.JobStatus)(int)job.Status,
                ProcessedCount = job.ProcessedCount,
                TotalCount = job.TotalCount,
                PercentComplete = job.TotalCount <= 0 ? 0 : (int)(job.ProcessedCount * 100.0 / job.TotalCount),
                ErrorMessage = job.ErrorMessage
            };
        }
    }
}
