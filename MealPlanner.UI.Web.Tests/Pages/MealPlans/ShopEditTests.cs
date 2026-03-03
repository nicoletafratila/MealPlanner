using System.Reflection;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages.MealPlans;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.MealPlans
{
    [TestFixture]
    public class ShopEditTests
    {
        private BunitContext _ctx = null!;
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<IProductCategoryService> _productCategoryServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _productCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_shopServiceMock.Object);
            _ctx.Services.AddSingleton(_productCategoryServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);

            _ctx.Services.AddBlazorBootstrap();
            _ctx.Services.AddLogging();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private void ArrangeCategories()
        {
            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], new Metadata()));
        }

        private IRenderedComponent<ShopEdit> RenderComponent(string? id = null)
        {
            return _ctx.Render<ShopEdit>(ps =>
            {
                if (id is not null)
                    ps.Add(p => p.Id, id);

                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithIdZero_CreatesNewShop()
        {
            // Arrange
            var categories = new PagedList<ProductCategoryModel>(
                [new() { Id = 1 }],
                new Metadata());

            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            // Act
            var cut = RenderComponent("0");

            // Assert
            Assert.That(cut.Instance.Shop, Is.Not.Null);
            Assert.That(cut.Instance.Shop!.Id, Is.Zero);

            _shopServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsShop()
        {
            // Arrange
            ArrangeCategories();

            var existing = new ShopEditModel([])
            {
                Id = 5,
                Name = "Loaded Shop"
            };

            _shopServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent("5");

            // Assert
            Assert.That(cut.Instance.Shop, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.Shop!.Id, Is.EqualTo(5));
                Assert.That(cut.Instance.Shop!.Name, Is.EqualTo("Loaded Shop"));
            }

            _shopServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_NullFromService_FallsBackToEmptyShop()
        {
            // Arrange
            ArrangeCategories();

            _shopServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync((ShopEditModel?)null);

            // Act
            var cut = RenderComponent("5");

            // Assert
            Assert.That(cut.Instance.Shop, Is.Not.Null);
            Assert.That(cut.Instance.Shop!.Id, Is.Zero); // constructed with default ctor
            _shopServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsShop_WhenIdIsZero()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _shopServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShopEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var shop = new ShopEditModel([]) { Id = 0, Name = "New Shop" };

            var method = typeof(ShopEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [shop])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(
                s => s.AddAsync(It.Is<ShopEditModel>(m => m.Name == "New Shop")),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("mealplans/shopsoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesShop_WhenIdIsNonZero()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existing = new ShopEditModel([])
            {
                Id = 5,
                Name = "Loaded Shop"
            };

            _shopServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _shopServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ShopEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var shop = new ShopEditModel([]) { Id = 5, Name = "Updated Shop" };

            var method = typeof(ShopEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [shop])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _shopServiceMock.Verify(
                s => s.UpdateAsync(It.Is<ShopEditModel>(m => m.Id == 5)),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeCategories();

            _shopServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShopEditModel>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("0");

            var shop = new ShopEditModel([]) { Id = 0, Name = "New Shop" };

            var method = typeof(ShopEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [shop])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Save failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Validation error"
            };

            _shopServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ShopEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent("0");

            var shop = new ShopEditModel([]) { Id = 0, Name = "New Shop" };

            var method = typeof(ShopEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [shop])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Validation error"),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenIdIsZero()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");
            cut.Instance.Shop = new ShopEditModel([]) { Id = 0 };

            var method = typeof(ShopEdit).GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_Deletes_WhenResponseSucceeded()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existing = new ShopEditModel([])
            {
                Id = 5,
                Name = "Loaded Shop"
            };

            _shopServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _shopServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var shop = new ShopEditModel([]) { Id = 5 };

            var method = typeof(ShopEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [shop])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _shopServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("mealplans/shopsoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            ArrangeCategories();

            var existing = new ShopEditModel([])
            {
                Id = 5,
                Name = "Loaded Shop"
            };

            _shopServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _shopServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent("5");

            var shop = new ShopEditModel([]) { Id = 5 };

            var method = typeof(ShopEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [shop])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            ArrangeCategories();

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Delete failed because of dependency"
            };

            var existing = new ShopEditModel([])
            {
                Id = 5,
                Name = "Loaded Shop"
            };

            _shopServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _shopServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent("5");

            var shop = new ShopEditModel([]) { Id = 5 };

            var method = typeof(ShopEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [shop])!;
                await task;
            });

            // Assert
            _shopServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed because of dependency"),
                Times.Once);
        }

        // ---------- CanMoveUp / CanMoveDown ----------
        [Test]
        public void CanMoveUp_True_WhenNotFirst()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");

            var seq = new List<ShopDisplaySequenceEditModel>
            {
                new() { Index = 1 },
                new() { Index = 2 },
                new() { Index = 3 }
            };

            cut.Instance.Shop.DisplaySequence = seq;

            var method = typeof(ShopEdit).GetMethod("CanMoveUp", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            var canMoveFirst = (bool)method!.Invoke(cut.Instance, [seq[0]])!;
            var canMoveSecond = (bool)method!.Invoke(cut.Instance, [seq[1]])!;

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(canMoveFirst, Is.False);
                Assert.That(canMoveSecond, Is.True);
            }
        }

        [Test]
        public void CanMoveDown_True_WhenNotLast()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");

            var seq = new List<ShopDisplaySequenceEditModel>
            {
                new() { Index = 1 },
                new() { Index = 2 },
                new() { Index = 3 }
            };

            cut.Instance.Shop.DisplaySequence = seq;

            var method = typeof(ShopEdit).GetMethod("CanMoveDown", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            var canMoveFirst = (bool)method!.Invoke(cut.Instance, [seq[0]])!;
            var canMoveLast = (bool)method!.Invoke(cut.Instance, [seq[2]])!;

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(canMoveFirst, Is.True);
                Assert.That(canMoveLast, Is.False);
            }
        }

        [Test]
        public void MoveUp_SwapsItem_WithPrevious()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");

            var seq = new List<ShopDisplaySequenceEditModel>
            {
                new() { Value = 1 },
                new() { Value = 2 },
                new() { Value = 3 }
            };

            cut.Instance.Shop.DisplaySequence = seq;

            var method = typeof(ShopEdit).GetMethod("MoveUp", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act: move middle item up (Value = 2)
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [seq[1]]));

            var newSeq = cut.Instance.Shop.DisplaySequence;

            // Assert: order should now be (2, 1, 3) by Value,
            // and Index should be reset 1..3 in that order.
            using (Assert.EnterMultipleScope())
            {
                Assert.That(newSeq[0].Value, Is.EqualTo(2));
                Assert.That(newSeq[1].Value, Is.EqualTo(1));
                Assert.That(newSeq[2].Value, Is.EqualTo(3));

                Assert.That(newSeq[0].Index, Is.EqualTo(1));
                Assert.That(newSeq[1].Index, Is.EqualTo(2));
                Assert.That(newSeq[2].Index, Is.EqualTo(3));
            }
        }

        [Test]
        public void MoveDown_SwapsItem_WithNext()
        {
            // Arrange
            ArrangeCategories();
            var cut = RenderComponent("0");

            var seq = new List<ShopDisplaySequenceEditModel>
            {
                new() { Value = 1 },
                new() {  Value = 2 },
                new() {  Value = 3 }
            };

            cut.Instance.Shop.DisplaySequence = seq;

            var method = typeof(ShopEdit).GetMethod("MoveDown", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act: move middle item down (Value = 2)
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [seq[1]]));

            var newSeq = cut.Instance.Shop.DisplaySequence;

            // Assert: order should now be (1, 3, 2) by Value,
            // and Index should be reset 1..3 in that order.
            using (Assert.EnterMultipleScope())
            {
                Assert.That(newSeq[0].Value, Is.EqualTo(1));
                Assert.That(newSeq[1].Value, Is.EqualTo(3));
                Assert.That(newSeq[2].Value, Is.EqualTo(2));

                Assert.That(newSeq[0].Index, Is.EqualTo(1));
                Assert.That(newSeq[1].Index, Is.EqualTo(2));
                Assert.That(newSeq[2].Index, Is.EqualTo(3));
            }
        }
    }
}