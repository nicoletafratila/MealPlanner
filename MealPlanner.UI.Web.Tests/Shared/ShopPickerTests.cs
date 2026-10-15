using System.Reflection;
using Bunit;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class ShopPickerTests
    {
        private BunitContext _ctx = null!;

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

        private IRenderedComponent<ShopPicker> RenderComponent(
            IEnumerable<ShopModel>? shops = null,
            string? selectedShopId = null,
            string placeholder = "Choose...",
            EventCallback<ChangeEventArgs> onChanged = default)
        {
            return _ctx.Render<ShopPicker>(ps =>
            {
                ps.Add(p => p.Id, "Shop");
                ps.Add(p => p.Shops, shops);
                ps.Add(p => p.SelectedShopId, selectedShopId);
                ps.Add(p => p.Placeholder, placeholder);
                ps.Add(p => p.OnChanged, onChanged);
            });
        }

        private static object? GetPrivate(ShopPicker instance, string name) =>
            typeof(ShopPicker).GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance);

        // ---------- SelectedShop ----------
        [Test]
        public void SelectedShop_ReturnsMatchingShop_WhenIdMatches()
        {
            // Arrange
            var shop1 = new ShopModel(Guid.NewGuid(), "Lidl");
            var shop2 = new ShopModel(Guid.NewGuid(), "Kaufland");
            var cut = RenderComponent([shop1, shop2], shop2.Id.ToString());

            // Act
            var selected = GetPrivate(cut.Instance, "SelectedShop");

            // Assert
            Assert.That(selected, Is.SameAs(shop2));
        }

        [Test]
        public void SelectedShop_ReturnsNull_WhenNoIdMatches()
        {
            // Arrange
            var shop1 = new ShopModel(Guid.NewGuid(), "Lidl");
            var cut = RenderComponent([shop1], Guid.NewGuid().ToString());

            // Act
            var selected = GetPrivate(cut.Instance, "SelectedShop");

            // Assert
            Assert.That(selected, Is.Null);
        }

        [Test]
        public void SelectedShop_ReturnsNull_WhenShopsIsNull()
        {
            // Arrange
            var cut = RenderComponent(null, Guid.NewGuid().ToString());

            // Act
            var selected = GetPrivate(cut.Instance, "SelectedShop");

            // Assert
            Assert.That(selected, Is.Null);
        }

        // ---------- FilteredShops / OnFilterInput ----------
        [Test]
        public void FilteredShops_ReturnsAllShops_WhenFilterTextEmpty()
        {
            // Arrange
            var shop1 = new ShopModel(Guid.NewGuid(), "Lidl");
            var shop2 = new ShopModel(Guid.NewGuid(), "Kaufland");
            var cut = RenderComponent([shop1, shop2]);

            // Act
            var filtered = (IEnumerable<ShopModel>?)GetPrivate(cut.Instance, "FilteredShops");

            // Assert
            Assert.That(filtered, Is.EquivalentTo(new[] { shop1, shop2 }));
        }

        [Test]
        public void OnFilterInput_FiltersShops_ByNameCaseInsensitive()
        {
            // Arrange
            var shop1 = new ShopModel(Guid.NewGuid(), "Lidl");
            var shop2 = new ShopModel(Guid.NewGuid(), "Kaufland");
            var cut = RenderComponent([shop1, shop2]);

            var onFilterInput = typeof(ShopPicker).GetMethod("OnFilterInput", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            cut.InvokeAsync(() => onFilterInput.Invoke(cut.Instance, [new ChangeEventArgs { Value = "lid" }]));
            var filtered = (IEnumerable<ShopModel>?)GetPrivate(cut.Instance, "FilteredShops");

            // Assert
            Assert.That(filtered, Is.EquivalentTo(new[] { shop1 }));
        }

        [Test]
        public void OnFilterInput_NullValue_SetsFilterTextToEmpty_AndShowsAllShops()
        {
            // Arrange
            var shop1 = new ShopModel(Guid.NewGuid(), "Lidl");
            var cut = RenderComponent([shop1]);

            var onFilterInput = typeof(ShopPicker).GetMethod("OnFilterInput", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            cut.InvokeAsync(() => onFilterInput.Invoke(cut.Instance, [new ChangeEventArgs { Value = null }]));
            var filterText = (string?)GetPrivate(cut.Instance, "FilterText");
            var filtered = (IEnumerable<ShopModel>?)GetPrivate(cut.Instance, "FilteredShops");

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(filterText, Is.EqualTo(string.Empty));
                Assert.That(filtered, Is.EquivalentTo(new[] { shop1 }));
            }
        }

        // ---------- SelectAsync ----------
        [Test]
        public async Task SelectAsync_WithShop_InvokesOnChanged_WithShopId_AndClearsFilterText()
        {
            // Arrange
            ChangeEventArgs? received = null;
            var callback = EventCallback.Factory.Create<ChangeEventArgs>(this, e => received = e);

            var shop = new ShopModel(Guid.NewGuid(), "Lidl");
            var cut = RenderComponent([shop], onChanged: callback);

            var onFilterInput = typeof(ShopPicker).GetMethod("OnFilterInput", BindingFlags.Instance | BindingFlags.NonPublic)!;
            await cut.InvokeAsync(() => onFilterInput.Invoke(cut.Instance, [new ChangeEventArgs { Value = "lid" }]));

            var selectAsync = typeof(ShopPicker).GetMethod("SelectAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)selectAsync.Invoke(cut.Instance, [shop])!;
                await task;
            });

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(received, Is.Not.Null);
                Assert.That(received!.Value, Is.EqualTo(shop.Id.ToString()));
                Assert.That((string?)GetPrivate(cut.Instance, "FilterText"), Is.EqualTo(string.Empty));
            }
        }

        [Test]
        public async Task SelectAsync_WithNull_InvokesOnChanged_WithEmptyString()
        {
            // Arrange
            ChangeEventArgs? received = null;
            var callback = EventCallback.Factory.Create<ChangeEventArgs>(this, e => received = e);

            var cut = RenderComponent(onChanged: callback);

            var selectAsync = typeof(ShopPicker).GetMethod("SelectAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)selectAsync.Invoke(cut.Instance, [null])!;
                await task;
            });

            // Assert
            Assert.That(received, Is.Not.Null);
            Assert.That(received!.Value, Is.EqualTo(string.Empty));
        }

        // ---------- Rendering ----------
        [Test]
        public void Render_WithSelectedShop_ShowsNameAndThumbnail()
        {
            // Arrange
            var shop = new ShopModel(Guid.NewGuid(), "Lidl") { ThumbnailUrl = "data:image/jpg;base64,AAA" };

            // Act
            var cut = RenderComponent([shop], shop.Id.ToString());

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Markup, Does.Contain("Lidl"));
                Assert.That(cut.Markup, Does.Contain("data:image/jpg;base64,AAA"));
            }
        }

        [Test]
        public void Render_WithoutSelectedShop_ShowsPlaceholder()
        {
            // Act
            var cut = RenderComponent(placeholder: "Pick a shop");

            // Assert
            Assert.That(cut.Markup, Does.Contain("Pick a shop"));
        }

        [Test]
        public void Render_ShopWithoutThumbnail_FallsBackToDefaultImage()
        {
            // Arrange
            var shop = new ShopModel(Guid.NewGuid(), "Lidl");

            // Act
            var cut = RenderComponent([shop], shop.Id.ToString());

            // Assert
            Assert.That(cut.Markup, Does.Contain("/Images/shop.jpg"));
        }
    }
}
