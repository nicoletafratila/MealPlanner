namespace RecipeBook.Api.Services
{
    /// <summary>
    /// Executes a queued share-all job: clones the owner's recipes (and the products they reference) into new records
    /// owned by the job's target user, reporting progress as it goes.
    /// </summary>
    public interface IRecipeJobProcessor
    {
        Task ProcessAsync(Guid jobId, CancellationToken cancellationToken);
    }
}
