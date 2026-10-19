using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipeBook.Api.Controllers;
using RecipeBook.Api.Features.Job.Queries.GetStatus;
using RecipeBook.Api.Features.Job.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Controllers
{
    [TestFixture]
    public class JobControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private JobController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new JobController(_senderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Test]
        public async Task GetStatusAsync_SendsGetStatusQuery()
        {
            var jobId = Guid.NewGuid();
            var model = new JobStatusModel { JobId = jobId };

            _senderMock
                .Setup(m => m.Send(It.Is<GetStatusQuery>(q => q.JobId == jobId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);

            var result = await _controller.GetStatusAsync(jobId, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(model));

            _senderMock.Verify(m => m.Send(It.IsAny<GetStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SearchAsync_InvalidPageParams_ReturnsBadRequest()
        {
            var result = await _controller.SearchAsync(null, null, "abc", "1", CancellationToken.None);
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SearchAsync_ValidParams_SendsSearchQuery()
        {
            var paged = new PagedList<JobModel>([], new Metadata());
            SearchQuery? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<PagedList<JobModel>>, CancellationToken>((q, _) => captured = (SearchQuery)q)
                .ReturnsAsync(paged);

            var result = await _controller.SearchAsync(null, null, "10", "2", CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok!.Value, Is.SameAs(paged));
                Assert.That(captured, Is.Not.Null);
            }
            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured!.QueryParameters!.PageSize, Is.EqualTo(10));
                Assert.That(captured!.QueryParameters!.PageNumber, Is.EqualTo(2));
            }

            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
