using Common.Pagination;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public interface IJobService
    {
        Task<JobStatusModel?> GetStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
        Task<PagedList<JobModel>?> SearchAsync(
            QueryParameters<JobModel>? queryParameters = null,
            CancellationToken cancellationToken = default);
    }
}
