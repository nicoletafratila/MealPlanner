using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RecipeBook.Api.Hubs.Resources;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Hubs
{
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope)]
    public class RecipeShareProgressHub(
        IJobRepository jobRepository,
        ICurrentUserService currentUserService) : Hub
    {
        public async Task JoinJobAsync(Guid jobId)
        {
            var job = await jobRepository.GetByIdAsync(jobId, Context.ConnectionAborted);
            if (job is null || job.OwnerUserId != currentUserService.UserId)
                throw new HubException(RecipeShareProgressHubMessages.JobNotFoundOrNotOwned);

            await Groups.AddToGroupAsync(Context.ConnectionId, jobId.ToString());
        }
    }
}
