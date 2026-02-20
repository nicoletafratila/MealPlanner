using BlazorBootstrap;
using Bunit;
using MealPlanner.UI.Web.Shared;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class GridTemplateTests
    {
        private BunitContext _ctx = null!;

        private static readonly GridDataProviderDelegate<TestItem> DefaultProvider = ctx =>
             Task.FromResult(new GridDataProviderResult<TestItem>
             {
                 Data = new List<TestItem>(),
                 TotalCount = 0
             });

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        [Test]
        public void RefreshDataAsync_DoesNothing_WhenGridReferenceIsNull()
        {
            // Arrange: plain instance, never rendered, so gridTemplateReference stays null
            var component = new GridTemplate<TestItem>();

            // Act & Assert: calling RefreshDataAsync should not throw
            Assert.DoesNotThrowAsync(async () => await component.RefreshDataAsync());
        }

        [Test]
        public void Defaults_AreSet_AsExpected()
        {
            // Arrange: render with minimal valid params (DataProvider required for inner Grid)
            var cut = _ctx.Render<GridTemplate<TestItem>>(parameters =>
            {
                parameters.Add(p => p.DataProvider, DefaultProvider);
            });

            // Assert defaults
            Assert.That(cut.Instance.TableGridClass, Is.EqualTo("table"));
            Assert.That(cut.Instance.HeaderRowCssClass, Is.EqualTo("bg-primary text-white"));
            Assert.That(cut.Instance.AllowPaging, Is.True);
        }

        [Test]
        public void Parameters_CanBeOverridden_ViaComponentParameters()
        {
            // Arrange: override parameters; still supply a DataProvider for inner Grid
            var cut = _ctx.Render<GridTemplate<TestItem>>(parameters =>
            {
                parameters.Add(p => p.DataProvider, DefaultProvider);
                parameters.Add(p => p.TableGridClass, "custom-table");
                parameters.Add(p => p.HeaderRowCssClass, "custom-header");
                parameters.Add(p => p.AllowPaging, false);
            });

            // Assert
            Assert.That(cut.Instance.TableGridClass, Is.EqualTo("custom-table"));
            Assert.That(cut.Instance.HeaderRowCssClass, Is.EqualTo("custom-header"));
            Assert.That(cut.Instance.AllowPaging, Is.False);
        }

        [Test]
        public void DataProvider_CanBeAssigned_ViaParameters()
        {
            // Arrange: custom provider
            GridDataProviderDelegate<TestItem> provider = async context =>
            {
                var items = new List<TestItem> { new TestItem { Name = "A" } };
                return await ValueTask.FromResult(new GridDataProviderResult<TestItem>
                {
                    Data = items,
                    TotalCount = items.Count
                });
            };

            var cut = _ctx.Render<GridTemplate<TestItem>>(parameters =>
            {
                parameters.Add(p => p.DataProvider, provider);
            });

            // Assert
            Assert.That(cut.Instance.DataProvider, Is.SameAs(provider));
        }
    }
}
