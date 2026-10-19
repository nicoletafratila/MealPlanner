using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    /// <summary>
    /// Connects to the RecipeBook API's share-progress hub for a single share-all job and raises
    /// <see cref="ProgressReceived"/> as updates arrive.
    /// </summary>
    public interface IRecipeShareProgressClient : IAsyncDisposable
    {
        event Action<JobStatusModel>? ProgressReceived;

        Task ConnectAsync(Guid jobId, CancellationToken cancellationToken = default);
    }
}
