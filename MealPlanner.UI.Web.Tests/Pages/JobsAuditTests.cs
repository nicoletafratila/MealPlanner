using System.Reflection;
using BlazorBootstrap;
using Blazored.SessionStorage;
using Bunit;
using Common.Models;
using Common.Pagination;
using MealPlanner.UI.Web.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Enums;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages
{
    [TestFixture]
    public class JobsAuditTests
    {
        private Mock<IJobService> _jobServiceMock = null!;
        private Mock<ILogger<JobsAudit>> _loggerMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private BunitContext _ctx = null!;

        private static JobModel CreateJob(string ownerUserId = "user1", JobStatus status = JobStatus.Completed)
        {
            return new JobModel
            {
                Id = Guid.NewGuid(),
                OwnerUserId = ownerUserId,
                TargetUserId = "user2",
                Status = status,
                TotalCount = 5,
                ProcessedCount = 5,
                PercentComplete = 100,
                CreatedAt = DateTime.UtcNow
            };
        }

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _jobServiceMock = new Mock<IJobService>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<JobsAudit>>(MockBehavior.Loose);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);

            _jobServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<JobModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<JobModel>([], new Metadata()));

            _sessionStorageMock
                .Setup(s => s.GetItemAsync<string?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            _ctx.Services.AddSingleton(_jobServiceMock.Object);
            _ctx.Services.AddSingleton(_loggerMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _jobServiceMock.Reset();
            _loggerMock.Reset();
            _sessionStorageMock.Reset();
        }

        private IRenderedComponent<JobsAudit> Render() => _ctx.Render<JobsAudit>();

        private static object InvokePrivate(JobsAudit instance, string methodName, params object?[] args)
        {
            var method = typeof(JobsAudit).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"{methodName} method not found via reflection.");
            return method!.Invoke(instance, args)!;
        }

        private static void SetPrivateProperty(JobsAudit instance, string propertyName, object? value)
        {
            var property = typeof(JobsAudit).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, $"{propertyName} property not found via reflection.");
            property!.SetValue(instance, value);
        }

        [Test]
        public async Task DataProviderAsync_CallsSearchAsync_SavesQuery_InSessionStorage_AndReturnsData()
        {
            var items = new List<JobModel> { CreateJob(), CreateJob("user3") };
            var paged = new PagedList<JobModel>(items, new Metadata { PageNumber = 1, PageSize = 20, TotalCount = 2 });

            _jobServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<JobModel>>(), CancellationToken.None))
                .ReturnsAsync(paged);

            var cut = Render();
            var request = new GridDataProviderRequest<JobModel> { PageNumber = 1, PageSize = 10 };

            var result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<JobModel>>)InvokePrivate(cut.Instance, "DataProviderAsync", request);
                return await task;
            });

            _jobServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<JobModel>>(q => q.PageNumber == 1 && q.PageSize == 10), CancellationToken.None),
                Times.Exactly(2));

            _sessionStorageMock.Verify(
                s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Data!.Count(), Is.EqualTo(2));
                Assert.That(result.TotalCount, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task DataProviderAsync_LogsError_AndReturnsEmpty_WhenSearchThrows()
        {
            _jobServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<JobModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var cut = Render();
            var request = new GridDataProviderRequest<JobModel> { PageNumber = 1, PageSize = 20 };

            var result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<JobModel>>)InvokePrivate(cut.Instance, "DataProviderAsync", request);
                return await task;
            });

            Assert.That(result.TotalCount, Is.Zero);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v != null && v.ToString()!.Contains("Failed to load job history.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task DataProviderAsync_IncludesStatusAndDateRangeFilters_WhenSet()
        {
            var cut = Render();

            SetPrivateProperty(cut.Instance, "StatusFilter", "Failed");
            SetPrivateProperty(cut.Instance, "CreatedFromFilter", new DateTime(2026, 1, 1));
            SetPrivateProperty(cut.Instance, "CreatedToFilter", new DateTime(2026, 1, 31));

            var request = new GridDataProviderRequest<JobModel> { PageNumber = 1, PageSize = 20 };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<JobModel>>)InvokePrivate(cut.Instance, "DataProviderAsync", request);
                await task;
            });

            _jobServiceMock.Verify(
                s => s.SearchAsync(
                    It.Is<QueryParameters<JobModel>>(q =>
                        q.Filters!.Any(f => f.PropertyName == "Status" && (string)f.Value! == "Failed") &&
                        q.Filters!.Any(f => f.PropertyName == "CreatedAt" && f.Operator == Common.Pagination.FilterOperator.GreaterThanOrEquals) &&
                        q.Filters!.Any(f => f.PropertyName == "CreatedAt" && f.Operator == Common.Pagination.FilterOperator.LessThanOrEquals)),
                    CancellationToken.None),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task OnCustomFilterChangedAsync_ResetsToFirstPage_InSessionStorage()
        {
            string? savedJson = null;
            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => savedJson = json)
                .Returns(ValueTask.CompletedTask);

            var cut = Render();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)InvokePrivate(cut.Instance, "OnCustomFilterChangedAsync");
                await task;
            });

            Assert.That(savedJson, Does.Contain("\"PageNumber\":1"));
        }
    }
}
