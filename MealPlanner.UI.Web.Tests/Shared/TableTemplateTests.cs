using Bunit;
using Common.Models;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class TableTemplateTests : BunitContext
    {
        private sealed class TestItem : BaseModel
        {
            public string Name { get; set; } = string.Empty;
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
            return Render<TableTemplate<TestItem>>(parameters =>
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

            // Assert
            Assert.That(instance.SelectedItem, Is.Null);
            Assert.That(itemA.IsSelected, Is.False);
            Assert.That(callbackItem, Is.Null);
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

            // Assert
            Assert.That(instance.SelectedItem, Is.SameAs(itemA));
            Assert.That(itemA.IsSelected, Is.True);
            Assert.That(itemB.IsSelected, Is.False);
            Assert.That(callbackCount, Is.EqualTo(0));
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

            // Assert
            Assert.That(instance.SelectedItem, Is.SameAs(itemB));
            Assert.That(itemA.IsSelected, Is.False);
            Assert.That(itemB.IsSelected, Is.True);
            Assert.That(itemC.IsSelected, Is.False);
            Assert.That(callbackItem, Is.SameAs(itemB));
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

            // Assert
            Assert.That(instance.SelectedItem, Is.SameAs(items[2]));
            Assert.That(items[0].IsSelected, Is.False);
            Assert.That(items[1].IsSelected, Is.False);
            Assert.That(items[2].IsSelected, Is.True);
        }
    }
}
