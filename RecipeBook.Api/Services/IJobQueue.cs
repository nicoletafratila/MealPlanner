namespace RecipeBook.Api.Services
{
    /// <summary>
    /// Queues share-all job ids so the dispatcher wakes up immediately instead of waiting for its polling fallback.
    /// </summary>
    public interface IJobQueue
    {
        void Enqueue(Guid jobId);

        IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken);
    }
}
