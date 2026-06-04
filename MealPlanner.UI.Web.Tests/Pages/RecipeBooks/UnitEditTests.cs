using System.Reflection;using Bunit; using Common.Models; using Common.UI; using MealPlanner.UI.Web.Pages.RecipeBooks; using Microsoft.AspNetCore.Components; using Microsoft.Extensions.DependencyInjection; using Moq; using RecipeBook.Services.Http; using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class UnitEditTests
    {
        private BunitContext _ctx = null!;
        private Mock<IUnitService> _unitServiceMock = null!;
        private Mock<IMessageComponent> _messageMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _messageMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_unitServiceMock.Object);
            _ctx.Services.AddScoped<BlazorBootstrap.BreadcrumbService>();
            _ctx.Services.AddScoped<BlazorBootstrap.ModalService>();
            _ctx.Services.AddScoped<BlazorBootstrap.PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);
            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.confirmDialog.show", _ => true);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _unitServiceMock.Reset();
            _messageMock.Reset();
        }

        private IRenderedComponent<UnitEdit> RenderWithMessageComponent(string? id = null)
        {
            return _ctx.Render<UnitEdit>(parameters =>
            {
                parameters.Add(p => p.Id, id);
                parameters.AddCascadingValue("MessageComponent", _messageMock.Object);
            });
        }

        // ---------- Initialization ----------
        [Test]
        public void OnInitializedAsync_WithNullOrEmptyId_CreatesNewUnit()
        {
            var cut = RenderWithMessageComponent(null);

            Assert.That(cut.Instance.Unit, Is.Not.Null);
            Assert.That(cut.Instance.Unit.Id, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void OnInitializedAsync_WithInvalidId_CreatesNewUnit()
        {
            var cut = RenderWithMessageComponent("not-a-guid");

            Assert.That(cut.Instance.Unit, Is.Not.Null);
            Assert.That(cut.Instance.Unit.Id, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void OnInitializedAsync_WithExistingId_LoadsUnit()
        {
            var id = Guid.NewGuid();
            var loaded = new UnitEditModel { Id = id };

            _unitServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(loaded);

            var cut = RenderWithMessageComponent(id.ToString());

            Assert.That(cut.Instance.Unit, Is.Not.Null);
            Assert.That(cut.Instance.Unit.Id, Is.EqualTo(id));
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_WhenIdEmpty_CallsAddAsync_AndShowsInfo_AndNavigates()
        {
            var unit = new UnitEditModel { Id = Guid.Empty };

            _unitServiceMock
                .Setup(s => s.AddAsync(unit, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent();

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [unit])!;
                await task;
            });

            _unitServiceMock.Verify(s => s.AddAsync(unit, CancellationToken.None), Times.Once);
            _messageMock.Verify(m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/unitsoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_WhenIdNotEmpty_CallsUpdateAsync_AndShowsInfo_AndNavigates()
        {
            var id = Guid.NewGuid();
            var unit = new UnitEditModel { Id = id };

            _unitServiceMock
               .Setup(s => s.GetEditAsync(unit.Id, CancellationToken.None))
               .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.UpdateAsync(unit, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent(id.ToString());

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [unit])!;
                await task;
            });

            _unitServiceMock.Verify(s => s.UpdateAsync(unit, CancellationToken.None), Times.Once);
            _messageMock.Verify(m => m.ShowInfoAsync("Data has been saved successfully", It.IsAny<string>(), CancellationToken.None), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/unitsoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_WhenResponseFailed_ShowsError()
        {
            var id = Guid.NewGuid();
            var unit = new UnitEditModel { Id = id };
            var response = new CommandResponse { Succeeded = false, Message = "save failed" };

            _unitServiceMock
               .Setup(s => s.GetEditAsync(unit.Id, CancellationToken.None))
               .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.UpdateAsync(unit, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent(id.ToString());

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [unit])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowErrorAsync("save failed", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_WhenResponseNull_ShowsGenericError()
        {
            var unit = new UnitEditModel { Id = Guid.Empty };

            _unitServiceMock
                .Setup(s => s.AddAsync(unit, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderWithMessageComponent();

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [unit])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowErrorAsync("Save failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        // ---------- DeleteCoreAsync ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndNavigates()
        {
            var id = Guid.NewGuid();
            var unit = new UnitEditModel { Id = id };

            _unitServiceMock
                .Setup(s => s.GetEditAsync(unit.Id, CancellationToken.None))
                .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent(id.ToString());

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [unit])!;
                await task;
            });

            _unitServiceMock.Verify(s => s.DeleteAsync(unit.Id, CancellationToken.None), Times.Once);
            _messageMock.Verify(m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/unitsoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseFailed_ShowsError()
        {
            var id = Guid.NewGuid();
            var unit = new UnitEditModel { Id = id };
            var response = new CommandResponse { Succeeded = false, Message = "delete failed" };

            _unitServiceMock
               .Setup(s => s.GetEditAsync(unit.Id, CancellationToken.None))
               .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent(id.ToString());

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [unit])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowErrorAsync("delete failed", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseNull_ShowsGenericError()
        {
            var id = Guid.NewGuid();
            var unit = new UnitEditModel { Id = id };

            _unitServiceMock
               .Setup(s => s.GetEditAsync(unit.Id, CancellationToken.None))
               .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderWithMessageComponent(id.ToString());

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [unit])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }
    }
}
