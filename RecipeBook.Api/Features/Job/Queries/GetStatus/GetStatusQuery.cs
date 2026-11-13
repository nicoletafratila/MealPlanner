using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Job.Queries.GetStatus
{
    /// <summary>
    /// Query to retrieve the current progress of a job.
    /// </summary>
    public record GetStatusQuery(Guid JobId) : IRequest<JobStatusModel?>;
}
