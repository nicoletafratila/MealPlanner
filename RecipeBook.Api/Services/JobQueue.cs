using System.Threading.Channels;

namespace RecipeBook.Api.Services
{
    public class JobQueue : IJobQueue
    {
        private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

        public void Enqueue(Guid jobId) => _channel.Writer.TryWrite(jobId);

        public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken) =>
            _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
