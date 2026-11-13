using System.Reflection;
using BlazorBootstrap;
using Blazored.SessionStorage;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.Services;
using Common.UI;
using MealPlanner.UI.Web.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages
{
    [TestFixture]
    public class AuditTrailTests
    {
        private Mock<ILoggerService> _loggerServiceMock = null!;
        private Mock<ILogger<AuditTrail>> _loggerMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;
        private BunitContext _ctx = null!;

        private static LogModel CreateLog(string message = "Test log", string level = "Information")
        {
            return new LogModel
            {
                Id = Guid.NewGuid(),
                Message = message,
                MessageTemplate = message,
                Level = level,
                Timestamp = DateTime.UtcNow,
                Exception = string.Empty,
                Properties = "{}"
            };
        }

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _loggerServiceMock = new Mock<ILoggerService>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<AuditTrail>>(MockBehavior.Loose);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _loggerServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<LogModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<LogModel>([], new Metadata()));

            _sessionStorageMock
                .Setup(s => s.GetItemAsync<string?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            _ctx.Services.AddSingleton(_loggerServiceMock.Object);
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
            _loggerServiceMock.Reset();
            _loggerMock.Reset();
            _sessionStorageMock.Reset();
            _messageComponentMock.Reset();
        }

        private IRenderedComponent<AuditTrail> RenderWithMessageComponent()
        {
            return _ctx.Render<AuditTrail>(parameters =>
            {
                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        private static object InvokePrivate(AuditTrail instance, string methodName, params object?[] args)
        {
            var method = typeof(AuditTrail).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"{methodName} method not found via reflection.");
            return method!.Invoke(instance, args)!;
        }

        private static void SetPrivateProperty(AuditTrail instance, string propertyName, object? value)
        {
            var property = typeof(AuditTrail).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, $"{propertyName} property not found via reflection.");
            property!.SetValue(instance, value);
        }

        // ---------- DataProviderAsync ----------
        [Test]
        public async Task DataProviderAsync_CallsSearchAsync_SavesQuery_InSessionStorage_AndReturnsData()
        {
            var items = new List<LogModel> { CreateLog(), CreateLog("Second log") };
            var metadata = new Metadata { PageNumber = 1, PageSize = 20, TotalCount = 2 };
            var paged = new PagedList<LogModel>(items, metadata);

            _loggerServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<LogModel>>(), CancellationToken.None))
                .ReturnsAsync(paged);

            var cut = RenderWithMessageComponent();

            var request = new GridDataProviderRequest<LogModel> { PageNumber = 1, PageSize = 10 };

            var result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<LogModel>>)InvokePrivate(cut.Instance, "DataProviderAsync", request);
                return await task;
            });

            _loggerServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<LogModel>>(q => q.PageNumber == 1 && q.PageSize == 10), CancellationToken.None),
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
        public async Task DataProviderAsync_LogsError_AndReturnsEmpty_WhenSearchAsyncThrows()
        {
            _loggerServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<LogModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var cut = RenderWithMessageComponent();
            var request = new GridDataProviderRequest<LogModel> { PageNumber = 1, PageSize = 20 };

            var result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<LogModel>>)InvokePrivate(cut.Instance, "DataProviderAsync", request);
                return await task;
            });

            Assert.That(result.TotalCount, Is.Zero);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v != null && v.ToString()!.Contains("Failed to load audit trail logs.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task DataProviderAsync_IncludesLevelAndDateRangeFilters_WhenSet()
        {
            var cut = RenderWithMessageComponent();

            SetPrivateProperty(cut.Instance, "LevelFilter", "Error");
            SetPrivateProperty(cut.Instance, "TimestampFromFilter", new DateTime(2026, 1, 1));
            SetPrivateProperty(cut.Instance, "TimestampToFilter", new DateTime(2026, 1, 31));

            var request = new GridDataProviderRequest<LogModel> { PageNumber = 1, PageSize = 20 };

            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<LogModel>>)InvokePrivate(cut.Instance, "DataProviderAsync", request);
                await task;
            });

            _loggerServiceMock.Verify(
                s => s.SearchAsync(
                    It.Is<QueryParameters<LogModel>>(q =>
                        q.Filters!.Any(f => f.PropertyName == "Level" && (string)f.Value! == "Error") &&
                        q.Filters!.Any(f => f.PropertyName == "Timestamp" && f.Operator == Common.Pagination.FilterOperator.GreaterThanOrEquals) &&
                        q.Filters!.Any(f => f.PropertyName == "Timestamp" && f.Operator == Common.Pagination.FilterOperator.LessThanOrEquals)),
                    CancellationToken.None),
                Times.AtLeastOnce);
        }

        // ---------- OnCustomFilterChangedAsync ----------
        [Test]
        public async Task OnCustomFilterChangedAsync_ResetsToFirstPage_InSessionStorage()
        {
            string? savedJson = null;
            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => savedJson = json)
                .Returns(ValueTask.CompletedTask);

            var cut = RenderWithMessageComponent();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)InvokePrivate(cut.Instance, "OnCustomFilterChangedAsync");
                await task;
            });

            Assert.That(savedJson, Does.Contain("\"PageNumber\":1"));
        }

        // ---------- DeleteAllLogsCoreAsync ----------
        [Test]
        public async Task DeleteAllLogsCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndRefreshesGrid()
        {
            _loggerServiceMock
                .Setup(s => s.DeleteLogsAsync(CancellationToken.None))
                .Returns(Task.CompletedTask);

            var cut = RenderWithMessageComponent();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)InvokePrivate(cut.Instance, "DeleteAllLogsCoreAsync");
                await task;
            });

            _loggerServiceMock.Verify(s => s.DeleteLogsAsync(CancellationToken.None), Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task DeleteAllLogsCoreAsync_WhenDeleteThrows_ShowsError_AndLogsError()
        {
            _loggerServiceMock
                .Setup(s => s.DeleteLogsAsync(CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("delete failed"));

            var cut = RenderWithMessageComponent();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)InvokePrivate(cut.Instance, "DeleteAllLogsCoreAsync");
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v != null && v.ToString()!.Contains("Failed to delete audit trail logs.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
