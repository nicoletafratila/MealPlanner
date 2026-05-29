using System.Reflection;using BlazorBootstrap; using Blazored.SessionStorage; using Bunit; using Common.Pagination; using Identity.Services.Core; using Identity.Shared.Models; using MealPlanner.UI.Web.Pages.Identities; using Microsoft.AspNetCore.Components; using Microsoft.Extensions.DependencyInjection; using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.Identities
{
    [TestFixture]
    public class UsersOverviewTests
    {
        private BunitContext _ctx = null!;
        private Mock<IApplicationUserService> _serviceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _serviceMock = new Mock<IApplicationUserService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);

            _ctx.Services.AddSingleton(_serviceMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);

            _serviceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ApplicationUserModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ApplicationUserModel>([], new Metadata()));

            _sessionStorageMock
                .Setup(s => s.GetItemAsync<string?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _serviceMock.Reset();
            _sessionStorageMock.Reset();
        }

        private IRenderedComponent<UsersOverview> RenderComponent()
        {
            return _ctx.Render<UsersOverview>();
        }

        // ---------- OnInitialized ----------

        [Test]
        public void OnInitialized_RendersWithoutError()
        {
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav, Is.Not.Null);
        }

        // ---------- Update ----------

        [Test]
        public void Update_NavigatesToUserProfile_WithUsername()
        {
            var cut = RenderComponent();
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(UsersOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var model = new ApplicationUserModel { Username = "jdoe" };

            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [model]));

            Assert.That(nav.Uri, Does.EndWith("identities/userprofile/jdoe"));
        }

        [Test]
        public void Update_DoesNotNavigate_WhenItemIsNull()
        {
            var cut = RenderComponent();
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            var initialUri = nav.Uri;

            var method = typeof(UsersOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [null!]));

            Assert.That(nav.Uri, Is.EqualTo(initialUri));
        }

        // ---------- DataProviderAsync ----------

        [Test]
        public async Task DataProviderAsync_ReturnsEmpty_WhenServiceReturnsNull()
        {
            _serviceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ApplicationUserModel>>(), CancellationToken.None))
                .ReturnsAsync((PagedList<ApplicationUserModel>?)null);

            var cut = RenderComponent();

            var method = typeof(UsersOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ApplicationUserModel>
            {
                Filters = [],
                Sorting = [],
                PageNumber = 1,
                PageSize = 10
            };

            GridDataProviderResult<ApplicationUserModel> result = default!;

            await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ApplicationUserModel>>)method!.Invoke(cut.Instance, [request])!;
                result = await task;
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Data, Is.Empty);
                Assert.That(result.TotalCount, Is.Zero);
            }
        }

        [Test]
        public async Task DataProviderAsync_CallsService_SavesQuery_InSessionStorage_AndReturnsData()
        {
            var items = new List<ApplicationUserModel>
            {
                new() { Username = "user1" },
                new() { Username = "user2" }
            };

            var metadata = new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 };
            var paged = new PagedList<ApplicationUserModel>(items, metadata);

            _serviceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ApplicationUserModel>>(), CancellationToken.None))
                .ReturnsAsync(paged);

            var cut = RenderComponent();

            var method = typeof(UsersOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<ApplicationUserModel>
            {
                PageNumber = 1,
                PageSize = 10
            };

            var result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<ApplicationUserModel>>)method!
                    .Invoke(cut.Instance, [request])!;
                return await task;
            });

            _serviceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<ApplicationUserModel>>(q =>
                    q.PageNumber == 1 && q.PageSize == 10), CancellationToken.None),
                Times.Exactly(2));

            _sessionStorageMock.Verify(
                s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Data!.Count(), Is.EqualTo(2));
                Assert.That(result.TotalCount, Is.EqualTo(2));
            }
        }
    }
}
