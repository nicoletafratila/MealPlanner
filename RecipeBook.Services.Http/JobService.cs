using Common.Http;
using Common.Pagination;
using Common.Services;
using Microsoft.Extensions.Logging;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public class JobService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<JobService> logger)
        : ServiceBase(httpClient, tokenProvider), IJobService
    {
        private readonly string _controller = JobControllers.JobUrl;

        public async Task<JobStatusModel?> GetStatusAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            var url = string.Format($"{_controller}/{JobControllers.StatusRouteFormat}", jobId);
            try
            {
                return await GetAsync<JobStatusModel>(url, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job GetStatusAsync failed. JobId {JobId}", jobId); return null;
            }
        }

        public async Task<PagedList<JobModel>?> SearchAsync(
            QueryParameters<JobModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SearchAsync(_controller, queryParameters, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job SearchAsync failed."); return null;
            }
        }
    }
}
