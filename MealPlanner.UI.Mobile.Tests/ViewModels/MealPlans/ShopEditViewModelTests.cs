using System.Collections.ObjectModel;
using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.Shared.Resources;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.MealPlans
{
    [TestFixture]
    public class ShopEditViewModelTests
    {
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<IProductCategoryService> _categoryServiceMock = null!;
        private ShopEditViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _categoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _viewModel = new ShopEditViewModel(_shopServiceMock.Object, _categoryServiceMock.Object);
        }

        [Test]
        public void OnShopIdChanged_NewShop_BuildsDisplaySequenceFromCategories()
        {
            var categories = new List<ProductCategoryModel>
            {
                new(Guid.NewGuid(), "Dairy"),
                new(Guid.NewGuid(), "Bakery")
            };
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>(categories, Metadata.Create(1, 200, categories.Count)));

            _viewModel.ShopId = Guid.Empty.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.True);
                Assert.That(_viewModel.DisplaySequence, Has.Count.EqualTo(2));
                Assert.That(_viewModel.DisplaySequence[0].Value, Is.EqualTo(1));
                Assert.That(_viewModel.DisplaySequence[0].ProductCategory, Is.EqualTo(categories[0]));
                Assert.That(_viewModel.DisplaySequence[1].Value, Is.EqualTo(2));
                Assert.That(_viewModel.DisplaySequence[1].ProductCategory, Is.EqualTo(categories[1]));
                Assert.That(_viewModel.IsBusy, Is.False);
            }

            _shopServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void OnShopIdChanged_ExistingShop_LoadsModelFromService()
        {
            var id = Guid.NewGuid();
            var category = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var existing = new ShopEditModel
            {
                Id = id,
                Name = "Lidl",
                DisplaySequence = [new ShopDisplaySequenceEditModel(id, 1, category)]
            };
            _shopServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _viewModel.ShopId = id.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.False);
                Assert.That(_viewModel.Model.Name, Is.EqualTo("Lidl"));
                Assert.That(_viewModel.DisplaySequence, Has.Count.EqualTo(1));
                Assert.That(_viewModel.DisplaySequence[0].ProductCategory, Is.EqualTo(category));
            }

            _categoryServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void MoveUp_MovesItemUpAndResequencesValues()
        {
            var categoryA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var categoryB = new ProductCategoryModel(Guid.NewGuid(), "Bakery");
            var categoryC = new ProductCategoryModel(Guid.NewGuid(), "Produce");
            var itemA = new ShopDisplaySequenceEditModel(Guid.Empty, 1, categoryA);
            var itemB = new ShopDisplaySequenceEditModel(Guid.Empty, 2, categoryB);
            var itemC = new ShopDisplaySequenceEditModel(Guid.Empty, 3, categoryC);
            _viewModel.DisplaySequence = new ObservableCollection<ShopDisplaySequenceEditModel> { itemA, itemB, itemC };

            _viewModel.MoveUpCommand.Execute(itemB);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.DisplaySequence.Select(i => i.ProductCategory), Is.EqualTo(new[] { categoryB, categoryA, categoryC }));
                Assert.That(itemB.Value, Is.EqualTo(1));
                Assert.That(itemA.Value, Is.EqualTo(2));
                Assert.That(itemC.Value, Is.EqualTo(3));
            }
        }

        [Test]
        public void MoveUp_ItemAlreadyFirst_DoesNotChangeOrder()
        {
            var categoryA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var categoryB = new ProductCategoryModel(Guid.NewGuid(), "Bakery");
            var itemA = new ShopDisplaySequenceEditModel(Guid.Empty, 1, categoryA);
            var itemB = new ShopDisplaySequenceEditModel(Guid.Empty, 2, categoryB);
            _viewModel.DisplaySequence = new ObservableCollection<ShopDisplaySequenceEditModel> { itemA, itemB };

            _viewModel.MoveUpCommand.Execute(itemA);

            Assert.That(_viewModel.DisplaySequence.Select(i => i.ProductCategory), Is.EqualTo(new[] { categoryA, categoryB }));
        }

        [Test]
        public void MoveDown_MovesItemDownAndResequencesValues()
        {
            var categoryA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var categoryB = new ProductCategoryModel(Guid.NewGuid(), "Bakery");
            var categoryC = new ProductCategoryModel(Guid.NewGuid(), "Produce");
            var itemA = new ShopDisplaySequenceEditModel(Guid.Empty, 1, categoryA);
            var itemB = new ShopDisplaySequenceEditModel(Guid.Empty, 2, categoryB);
            var itemC = new ShopDisplaySequenceEditModel(Guid.Empty, 3, categoryC);
            _viewModel.DisplaySequence = new ObservableCollection<ShopDisplaySequenceEditModel> { itemA, itemB, itemC };

            _viewModel.MoveDownCommand.Execute(itemB);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.DisplaySequence.Select(i => i.ProductCategory), Is.EqualTo(new[] { categoryA, categoryC, categoryB }));
                Assert.That(itemA.Value, Is.EqualTo(1));
                Assert.That(itemC.Value, Is.EqualTo(2));
                Assert.That(itemB.Value, Is.EqualTo(3));
            }
        }

        [Test]
        public void MoveDown_ItemAlreadyLast_DoesNotChangeOrder()
        {
            var categoryA = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var categoryB = new ProductCategoryModel(Guid.NewGuid(), "Bakery");
            var itemA = new ShopDisplaySequenceEditModel(Guid.Empty, 1, categoryA);
            var itemB = new ShopDisplaySequenceEditModel(Guid.Empty, 2, categoryB);
            _viewModel.DisplaySequence = new ObservableCollection<ShopDisplaySequenceEditModel> { itemA, itemB };

            _viewModel.MoveDownCommand.Execute(itemB);

            Assert.That(_viewModel.DisplaySequence.Select(i => i.ProductCategory), Is.EqualTo(new[] { categoryA, categoryB }));
        }

        [Test]
        public async Task SaveAsync_NameMissing_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = string.Empty;
            _viewModel.DisplaySequence = new ObservableCollection<ShopDisplaySequenceEditModel>
            {
                new ShopDisplaySequenceEditModel(Guid.Empty, 1, new ProductCategoryModel(Guid.NewGuid(), "Dairy"))
            };

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.ShopNameRequired));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _shopServiceMock.Verify(
                s => s.AddAsync(It.IsAny<ShopEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _shopServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<ShopEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_DisplaySequenceEmpty_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "Lidl";
            _viewModel.DisplaySequence = [];

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.ShopRequiresCategoryOrder));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _shopServiceMock.Verify(
                s => s.AddAsync(It.IsAny<ShopEditModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_NewShopValid_CallsAddAsync()
        {
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], Metadata.Create(1, 200, 0)));
            _viewModel.ShopId = Guid.Empty.ToString();

            _viewModel.Model.Name = "Lidl";
            _viewModel.DisplaySequence = new ObservableCollection<ShopDisplaySequenceEditModel>
            {
                new ShopDisplaySequenceEditModel(Guid.Empty, 1, new ProductCategoryModel(Guid.NewGuid(), "Dairy"))
            };

            _shopServiceMock
                .Setup(s => s.AddAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current.GoToAsync runs after a successful save inside the try/catch, so the
            // resulting NullReferenceException in this test host is swallowed into ErrorMessage.
            // Only the service call is verified here.
            await _viewModel.SaveCommand.ExecuteAsync(null);

            _shopServiceMock.Verify(s => s.AddAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveAsync_ExistingShopValid_CallsUpdateAsync()
        {
            var id = Guid.NewGuid();
            var category = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            var existing = new ShopEditModel
            {
                Id = id,
                Name = "Lidl",
                DisplaySequence = [new ShopDisplaySequenceEditModel(id, 1, category)]
            };
            _shopServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);
            _viewModel.ShopId = id.ToString();

            _viewModel.Model.Name = "Lidl updated";

            _shopServiceMock
                .Setup(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.SaveCommand.ExecuteAsync(null);

            _shopServiceMock.Verify(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_WhenIsNew_ReturnsWithoutCallingService()
        {
            // DeleteAsync confirms via Shell.Current.DisplayAlertAsync before any try/catch, so
            // calling it past the IsNew guard would throw in this test host. Only the guard-clause
            // return path is exercised here.
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], Metadata.Create(1, 200, 0)));
            _viewModel.ShopId = Guid.Empty.ToString();
            Assert.That(_viewModel.IsNew, Is.True);

            await _viewModel.DeleteCommand.ExecuteAsync(null);

            _shopServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
