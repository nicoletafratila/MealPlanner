using RecipeBook.Api.Services;

namespace RecipeBook.Api.Tests.Services
{
    [TestFixture]
    public class JobQueueTests
    {
        private JobQueue _queue = null!;

        [SetUp]
        public void SetUp()
        {
            _queue = new JobQueue();
        }

        [Test]
        public async Task Enqueue_ThenReadAllAsync_YieldsTheEnqueuedId()
        {
            var jobId = Guid.NewGuid();
            _queue.Enqueue(jobId);

            using var cts = new CancellationTokenSource();
            var enumerator = _queue.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);

            Assert.That(await enumerator.MoveNextAsync(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(jobId));
        }

        [Test]
        public async Task Enqueue_MultipleIds_YieldsThemInOrder()
        {
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();
            _queue.Enqueue(first);
            _queue.Enqueue(second);

            using var cts = new CancellationTokenSource();
            var enumerator = _queue.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);

            Assert.That(await enumerator.MoveNextAsync(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(first));
            Assert.That(await enumerator.MoveNextAsync(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(second));
        }

        [Test]
        public async Task ReadAllAsync_CancelledToken_StopsEnumeration()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var ex = await Assert.CatchAsync(async () =>
            {
                await foreach (var _ in _queue.ReadAllAsync(cts.Token))
                {
                }
            });

            Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        }
    }
}
