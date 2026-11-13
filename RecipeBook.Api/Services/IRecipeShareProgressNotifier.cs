using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Services
{
    /// <summary>
    /// Pushes share-all job progress to any connected clients listening for a given job.
    /// </summary>
    public interface IRecipeShareProgressNotifier
    {
        Task NotifyAsync(Job job, CancellationToken cancellationToken);
    }
}
