using Common.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace RecipeBook.Services.Http.Tests
{
    [TestFixture]
    public class RecipeShareProgressClientTests
    {
        private static RecipeShareProgressClient CreateClient()
        {
            var configuration = new ConfigurationBuilder().Build();
            var tokenProvider = new Mock<ITokenProvider>(MockBehavior.Loose);
            return new RecipeShareProgressClient(configuration, tokenProvider.Object);
        }

        [Test]
        public async Task DisposeAsync_NeverConnected_DoesNotThrow()
        {
            var client = CreateClient();

            await Assert.DoesNotThrowAsync(async () => await client.DisposeAsync());
        }

        [Test]
        public void ProgressReceived_CanBeSubscribedAndUnsubscribed()
        {
            var client = CreateClient();
            void Handler(Shared.Models.JobStatusModel status)
            {
            }

            Assert.DoesNotThrow(() =>
            {
                client.ProgressReceived += Handler;
                client.ProgressReceived -= Handler;
            });
        }
    }
}
