using Bunit;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class TableTemplateTests
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

        private static RenderFragment<TestItem> CreateRowTemplate()
        {
            return item => builder =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item.Name);
                builder.CloseElement();
            };
        }

        private IRenderedComponent<TableTemplate<TestItem>> RenderWithData(
            IEnumerable<TestItem> data,
            TestItem? selectedItem = null,
            EventCallback<TestItem>? selectedItemChanged = null,
            bool showIndex = false)
        {
            return _ctx.Render<TableTemplate<TestItem>>(parameters =>
            {
                parameters.Add(p => p.Data, data);
                parameters.Add(p => p.ShowIndex, showIndex);
                parameters.Add(p => p.ColumnCount, showIndex ? 2 : 1); // header + index if shown
                parameters.Add(p => p.RowTemplate, CreateRowTemplate());

                if (selectedItem is not null)
                    parameters.Add(p => p.SelectedItem, selectedItem);

                if (selectedItemChanged is { } callback)
                    parameters.Add(p => p.SelectedItemChanged, callback);
            });
        }

        [Test]
        public async Task OnSelectedItemChangedAsync_DoesNothing_WhenItemIsNull()
        {
            // Arrange
            var itemA = new TestItem { Name = "A" };
            var data = new[] { itemA };

            TestItem? callbackItem = null;

            var cut = RenderWithData(
                data,
                selectedItemChanged: EventCallback.Factory.Create<TestItem>(
                    this,
                    i => callbackItem = i));

            var instance = cut.Instance;

            // Act
            await instance.OnSelectedItemChangedAsync(null!);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(instance.SelectedItem, Is.Null);
                Assert.That(itemA.IsSelected, Is.False);
                Assert.That(callbackItem, Is.Null);
            }
        }

        [Test]
        public async Task OnSelectedItemChangedAsync_DoesNothing_WhenSameItemSelected()
        {
            // Arrange
            var itemA = new TestItem { Name = "A", IsSelected = true };
            var itemB = new TestItem { Name = "B" };
            var data = new[] { itemA, itemB };

            var callbackCount = 0;

            var cut = RenderWithData(
                data,
                selectedItem: itemA,
                selectedItemChanged: EventCallback.Factory.Create<TestItem>(
                    this,
                    _ => callbackCount++));

            var instance = cut.Instance;

            // Act
            await instance.OnSelectedItemChangedAsync(itemA);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(instance.SelectedItem, Is.SameAs(itemA));
                Assert.That(itemA.IsSelected, Is.True);
                Assert.That(itemB.IsSelected, Is.False);
                Assert.That(callbackCount, Is.Zero);
            }
        }

        [Test]
        public async Task OnSelectedItemChangedAsync_UpdatesSelectedItem_SelectionFlags_AndInvokesCallback()
        {
            // Arrange
            var itemA = new TestItem { Name = "A" };
            var itemB = new TestItem { Name = "B" };
            var itemC = new TestItem { Name = "C", IsSelected = true };
            var data = new[] { itemA, itemB, itemC };

            TestItem? callbackItem = null;

            var cut = RenderWithData(
                data,
                selectedItem: itemC,
                selectedItemChanged: EventCallback.Factory.Create<TestItem>(
                    this,
                    i => callbackItem = i));

            var instance = cut.Instance;

            // Act
            await instance.OnSelectedItemChangedAsync(itemB);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(instance.SelectedItem, Is.SameAs(itemB));
                Assert.That(itemA.IsSelected, Is.False);
                Assert.That(itemB.IsSelected, Is.True);
                Assert.That(itemC.IsSelected, Is.False);
                Assert.That(callbackItem, Is.SameAs(itemB));
            }
        }

        [Test]
        public async Task OnSelectedItemChangedAsync_SetsOnlyClickedItemSelected_WhenDataPresent()
        {
            // Arrange
            var items = new[]
            {
                new TestItem { Name = "A" },
                new TestItem { Name = "B" },
                new TestItem { Name = "C" }
            };

            var cut = RenderWithData(items);
            var instance = cut.Instance;

            // Act
            await instance.OnSelectedItemChangedAsync(items[2]);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(instance.SelectedItem, Is.SameAs(items[2]));
                Assert.That(items[0].IsSelected, Is.False);
                Assert.That(items[1].IsSelected, Is.False);
                Assert.That(items[2].IsSelected, Is.True);
            }
        }

        // ---------- Drag reorder ----------
        [Test]
        public async Task OnDropAsync_InvokesOnReorder_WithDraggedAndTargetItems()
        {
            var itemA = new TestItem { Name = "A" };
            var itemB = new TestItem { Name = "B" };
            var data = new[] { itemA, itemB };

            (TestItem DraggedItem, TestItem TargetItem)? invokedWith = null;

            var cut = _ctx.Render<TableTemplate<TestItem>>(parameters =>
            {
                parameters.Add(p => p.Data, data);
                parameters.Add(p => p.RowTemplate, CreateRowTemplate());
                parameters.Add(p => p.EnableDragReorder, true);
                parameters.Add(p => p.OnReorder, EventCallback.Factory.Create<(TestItem, TestItem)>(
                    this,
                    r => invokedWith = r));
            });

            var instance = cut.Instance;

            instance.OnDragStart(itemA);
            await instance.OnDropAsync(itemB);

            Assert.That(invokedWith, Is.EqualTo((itemA, itemB)));
        }

        [Test]
        public async Task OnDropAsync_DoesNothing_WhenDroppedOnSameItem()
        {
            var itemA = new TestItem { Name = "A" };
            var data = new[] { itemA };

            var callbackCount = 0;

            var cut = _ctx.Render<TableTemplate<TestItem>>(parameters =>
            {
                parameters.Add(p => p.Data, data);
                parameters.Add(p => p.RowTemplate, CreateRowTemplate());
                parameters.Add(p => p.EnableDragReorder, true);
                parameters.Add(p => p.OnReorder, EventCallback.Factory.Create<(TestItem, TestItem)>(
                    this,
                    _ => callbackCount++));
            });

            var instance = cut.Instance;

            instance.OnDragStart(itemA);
            await instance.OnDropAsync(itemA);

            Assert.That(callbackCount, Is.Zero);
        }

        [Test]
        public async Task OnDropAsync_DoesNothing_WhenNoDragInProgress()
        {
            var itemA = new TestItem { Name = "A" };
            var data = new[] { itemA };

            var callbackCount = 0;

            var cut = _ctx.Render<TableTemplate<TestItem>>(parameters =>
            {
                parameters.Add(p => p.Data, data);
                parameters.Add(p => p.RowTemplate, CreateRowTemplate());
                parameters.Add(p => p.EnableDragReorder, true);
                parameters.Add(p => p.OnReorder, EventCallback.Factory.Create<(TestItem, TestItem)>(
                    this,
                    _ => callbackCount++));
            });

            var instance = cut.Instance;

            await instance.OnDropAsync(itemA);

            Assert.That(callbackCount, Is.Zero);
        }
    }
}
