using System.Reflection;
using Bunit;
using Common.Models;
using Common.UI;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Shared.Models;

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
        public void OnInitializedAsync_WithNullOrZeroId_CreatesNewUnit()
        {
            var cut = RenderWithMessageComponent(null);

            Assert.That(cut.Instance.Unit, Is.Not.Null);
            Assert.That(cut.Instance.Unit.Id, Is.EqualTo(0));
        }

        [Test]
        public void OnInitializedAsync_WithInvalidId_CreatesNewUnit()
        {
            var cut = RenderWithMessageComponent("not-a-number");

            Assert.That(cut.Instance.Unit, Is.Not.Null);
            Assert.That(cut.Instance.Unit.Id, Is.EqualTo(0));
        }

        [Test]
        public void OnInitializedAsync_WithExistingId_LoadsUnit()
        {
            const int id = 5;
            var loaded = new UnitEditModel { Id = id };

            _unitServiceMock
                .Setup(s => s.GetEditAsync(id))
                .ReturnsAsync(loaded);

            var cut = RenderWithMessageComponent(id.ToString());

            Assert.That(cut.Instance.Unit, Is.Not.Null);
            Assert.That(cut.Instance.Unit.Id, Is.EqualTo(id));
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_WhenIdZero_CallsAddAsync_AndShowsInfo_AndNavigates()
        {
            var unit = new UnitEditModel { Id = 0 };

            _unitServiceMock
                .Setup(s => s.AddAsync(unit))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent();

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { unit })!;
                await task;
            });

            _unitServiceMock.Verify(s => s.AddAsync(unit), Times.Once);
            _messageMock.Verify(m => m.ShowInfo("Data has been saved successfully"), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/unitsoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_WhenIdNonZero_CallsUpdateAsync_AndShowsInfo_AndNavigates()
        {
            var unit = new UnitEditModel { Id = 10 };

            _unitServiceMock
               .Setup(s => s.GetEditAsync(unit.Id))
               .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.UpdateAsync(unit))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent("10");

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { unit })!;
                await task;
            });

            _unitServiceMock.Verify(s => s.UpdateAsync(unit), Times.Once);
            _messageMock.Verify(m => m.ShowInfo("Data has been saved successfully"), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/unitsoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_WhenResponseFailed_ShowsError()
        {
            var unit = new UnitEditModel { Id = 10 };
            var response = new CommandResponse { Succeeded = false, Message = "save failed" };

            _unitServiceMock
               .Setup(s => s.GetEditAsync(unit.Id))
               .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.UpdateAsync(unit))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent("10");

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { unit })!;
                await task;
            });

            _messageMock.Verify(m => m.ShowError("save failed"), Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_WhenResponseNull_ShowsGenericError()
        {
            var unit = new UnitEditModel { Id = 0 };

            _unitServiceMock
                .Setup(s => s.AddAsync(unit))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderWithMessageComponent();

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { unit })!;
                await task;
            });

            _messageMock.Verify(m => m.ShowError("Save failed. Please try again."), Times.Once);
        }

        // ---------- DeleteCoreAsync ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndNavigates()
        {
            var unit = new UnitEditModel { Id = 7 };

            _unitServiceMock
                .Setup(s => s.GetEditAsync(unit.Id))
                .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent("7");

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { unit })!;
                await task;
            });

            _unitServiceMock.Verify(s => s.DeleteAsync(unit.Id), Times.Once);
            _messageMock.Verify(m => m.ShowInfo("Data has been deleted successfully"), Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/unitsoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseFailed_ShowsError()
        {
            var unit = new UnitEditModel { Id = 7 };
            var response = new CommandResponse { Succeeded = false, Message = "delete failed" };

            _unitServiceMock
               .Setup(s => s.GetEditAsync(unit.Id))
               .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent("7");

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { unit })!;
                await task;
            });

            _messageMock.Verify(m => m.ShowError("delete failed"), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseNull_ShowsGenericError()
        {
            var unit = new UnitEditModel { Id = 7 };

            _unitServiceMock
               .Setup(s => s.GetEditAsync(unit.Id))
               .ReturnsAsync(unit);
            _unitServiceMock
                .Setup(s => s.DeleteAsync(unit.Id))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderWithMessageComponent("7");

            cut.Instance.GetType()
                .GetProperty(nameof(UnitEdit.Unit))!
                .SetValue(cut.Instance, unit);

            var method = typeof(UnitEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { unit })!;
                await task;
            });

            _messageMock.Verify(m => m.ShowError("Delete failed. Please try again."), Times.Once);
        }
    }
}