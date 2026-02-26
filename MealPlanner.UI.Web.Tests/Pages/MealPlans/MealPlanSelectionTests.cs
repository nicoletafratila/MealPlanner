using System.Reflection;
using Blazored.Modal;
using Bunit;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages;
using MealPlanner.UI.Web.Pages.MealPlans;
using MealPlanner.UI.Web.Services.MealPlans;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.MealPlans
{
    [TestFixture]
    public class MealPlanSelectionTests
    {
        private BunitContext _ctx = null!;
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<IModalController> _modalControllerMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _modalControllerMock = new Mock<IModalController>(MockBehavior.Strict);

            _ctx.Services.AddSingleton(_mealPlanServiceMock.Object);
            _ctx.Services.AddBlazoredModal();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _mealPlanServiceMock.Reset();
            _modalControllerMock.Reset();
        }

        private IRenderedComponent<MealPlanSelection> RenderComponent()
        {
            return _ctx.Render<MealPlanSelection>();
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_LoadsMealPlans()
        {
            // Arrange
            var items = new List<MealPlanModel>
            {
                new() { Id = 1, Name = "Plan1" },
                new() { Id = 2, Name = "Plan2" }
            };

            var mealPlans = new PagedList<MealPlanModel>(
                items, new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 });

            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>()))
                .ReturnsAsync(mealPlans);

            // Act
            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            // Assert
            Assert.That(cut.Instance.MealPlans, Is.Not.Null);
            Assert.That(cut.Instance.MealPlans!.Items, Has.Count.EqualTo(2));

            _mealPlanServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>()),
                Times.Once);
        }

        // ---------- SaveAsync / CancelAsync using IModalController mock ----------
        [Test]
        public async Task SaveAsync_UsesModalController_WithMealPlanId()
        {
            // Arrange
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>()))
                .ReturnsAsync(new PagedList<MealPlanModel>(new List<MealPlanModel>(), new Metadata()));

            _modalControllerMock
                .Setup(m => m.CloseAsync(It.IsAny<object?>()))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent();

            // Inject the controller mock
            cut.Instance.ModalController = _modalControllerMock.Object;
            cut.Instance.MealPlanId = "42";

            var method = typeof(MealPlanSelection).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert
            _modalControllerMock.Verify(
                m => m.CloseAsync("42"),
                Times.Once);
        }

        [Test]
        public async Task CancelAsync_UsesModalController_Cancel()
        {
            // Arrange
            _mealPlanServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<MealPlanModel>>()))
                .ReturnsAsync(new PagedList<MealPlanModel>(new List<MealPlanModel>(), new Metadata()));

            _modalControllerMock
                .Setup(m => m.CancelAsync())
                .Returns(Task.CompletedTask);

            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            var method = typeof(MealPlanSelection).GetMethod("CancelAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert
            _modalControllerMock.Verify(m => m.CancelAsync(), Times.Once);
        }
    }
}