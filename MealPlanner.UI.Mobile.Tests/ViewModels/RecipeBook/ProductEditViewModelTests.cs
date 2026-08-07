using Common.Constants.Units;
using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class ProductEditViewModelTests
    {
        private Mock<IProductService> _productServiceMock = null!;
        private Mock<IProductCategoryService> _categoryServiceMock = null!;
        private Mock<IUnitService> _unitServiceMock = null!;
        private Mock<IRecipeCategoryService> _lookupRecipeCategoryServiceMock = null!;
        private Mock<IShopService> _lookupShopServiceMock = null!;
        private Mock<IProductService> _lookupProductServiceMock = null!;
        private Mock<IRecipeService> _lookupRecipeServiceMock = null!;
        private ProductEditViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _productServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _categoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _lookupRecipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _lookupShopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _lookupProductServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _lookupRecipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);

            var lookupDataService = new ReferenceDataCacheService(
                _lookupRecipeCategoryServiceMock.Object,
                _unitServiceMock.Object,
                _lookupProductServiceMock.Object,
                _categoryServiceMock.Object,
                _lookupShopServiceMock.Object,
                _lookupRecipeServiceMock.Object);

            _viewModel = new ProductEditViewModel(_productServiceMock.Object, lookupDataService);
        }

        private void SetupCategoriesAndUnits(List<ProductCategoryModel> categories, List<UnitModel> units)
        {
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>(categories, Metadata.Create(1, 100, categories.Count)));

            _unitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>(units, Metadata.Create(1, 100, units.Count)));

            _lookupRecipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], Metadata.Create(1, 100, 0)));

            _lookupShopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>([], Metadata.Create(1, 200, 0)));
        }

        [Test]
        public void OnProductIdChanged_NewId_SetsIsNewTrueAndLoadsCategoriesAndUnits()
        {
            var categories = new List<ProductCategoryModel> { new(Guid.NewGuid(), "Dairy") };
            var units = new List<UnitModel> { new(Guid.NewGuid(), "Kilogram", UnitType.Weight) };
            SetupCategoriesAndUnits(categories, units);

            _viewModel.ProductId = "0";

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.True);
                Assert.That(_viewModel.Categories, Has.Count.EqualTo(1));
                Assert.That(_viewModel.Units, Has.Count.EqualTo(1));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _productServiceMock.Verify(s => s.GetEditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void OnProductIdChanged_ExistingId_SetsIsNewFalseAndLoadsProductWithSelections()
        {
            var categoryId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var category = new ProductCategoryModel(categoryId, "Dairy");
            var unit = new UnitModel(unitId, "Kilogram", UnitType.Weight);
            SetupCategoriesAndUnits([category], [unit]);

            var editModel = new ProductEditModel(productId, "Milk", unitId, categoryId)
            {
                ImageContent = [1, 2, 3]
            };
            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync(editModel);

            _viewModel.ProductId = productId.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.False);
                Assert.That(_viewModel.Model.Name, Is.EqualTo("Milk"));
                Assert.That(_viewModel.SelectedCategory, Is.EqualTo(category));
                Assert.That(_viewModel.SelectedUnit, Is.EqualTo(unit));
                // ImageSource.FromStream doesn't reliably materialize in this headless test host,
                // so assert on the underlying bytes rather than the resulting ImageSource.
                Assert.That(_viewModel.Model.ImageContent, Is.EqualTo(new byte[] { 1, 2, 3 }));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public void OnProductIdChanged_ExistingIdWithoutImageContent_DoesNotSetProductImage()
        {
            var productId = Guid.NewGuid();
            SetupCategoriesAndUnits([], []);

            var editModel = new ProductEditModel(productId, "Milk", Guid.Empty, Guid.Empty);
            _productServiceMock
                .Setup(s => s.GetEditAsync(productId, CancellationToken.None))
                .ReturnsAsync(editModel);

            _viewModel.ProductId = productId.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.False);
                Assert.That(_viewModel.ProductImage, Is.Null);
            }
        }

        [Test]
        public void OnSelectedCategoryChanged_WithValue_UpdatesModelProductCategoryId()
        {
            var category = new ProductCategoryModel(Guid.NewGuid(), "Dairy");

            _viewModel.SelectedCategory = category;

            Assert.That(_viewModel.Model.ProductCategoryId, Is.EqualTo(category.Id));
        }

        [Test]
        public void OnSelectedCategoryChanged_NullValue_DoesNotChangeModel()
        {
            var originalId = _viewModel.Model.ProductCategoryId;

            _viewModel.SelectedCategory = null;

            Assert.That(_viewModel.Model.ProductCategoryId, Is.EqualTo(originalId));
        }

        [Test]
        public void OnSelectedUnitChanged_WithValue_UpdatesModelBaseUnitId()
        {
            var unit = new UnitModel(Guid.NewGuid(), "Kilogram", UnitType.Weight);

            _viewModel.SelectedUnit = unit;

            Assert.That(_viewModel.Model.BaseUnitId, Is.EqualTo(unit.Id));
        }

        [Test]
        public void OnSelectedUnitChanged_NullValue_DoesNotChangeModel()
        {
            var originalId = _viewModel.Model.BaseUnitId;

            _viewModel.SelectedUnit = null;

            Assert.That(_viewModel.Model.BaseUnitId, Is.EqualTo(originalId));
        }

        [Test]
        public async Task SaveAsync_NameMissing_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = string.Empty;
            _viewModel.Model.ImageContent = [1];
            _viewModel.SelectedCategory = new ProductCategoryModel(Guid.NewGuid(), "Dairy");

            await _viewModel.SaveCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.ProductNameRequired));
            _productServiceMock.Verify(s => s.AddAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _productServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_ImageMissing_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "Milk";
            _viewModel.Model.ImageContent = null;
            _viewModel.SelectedCategory = new ProductCategoryModel(Guid.NewGuid(), "Dairy");

            await _viewModel.SaveCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.ProductImageRequired));
            _productServiceMock.Verify(s => s.AddAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _productServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_CategoryMissing_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "Milk";
            _viewModel.Model.ImageContent = [1];
            _viewModel.SelectedCategory = null;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.ProductCategoryRequired));
            _productServiceMock.Verify(s => s.AddAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _productServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_WhenIsBusy_DoesNothing()
        {
            _viewModel.IsBusy = true;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.True);
            }
            _productServiceMock.Verify(s => s.AddAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _productServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_NewProductValid_CallsAddAsync()
        {
            _viewModel.IsNew = true;
            _viewModel.Model.Name = "Milk";
            _viewModel.Model.ImageContent = [1];
            _viewModel.SelectedCategory = new ProductCategoryModel(Guid.NewGuid(), "Dairy");

            _productServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductEditModel>(), CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current.GoToAsync is called after a successful add; it throws a
            // NullReferenceException in this test host, which is caught internally.
            await _viewModel.SaveCommand.ExecuteAsync(null);

            _productServiceMock.Verify(s => s.AddAsync(It.IsAny<ProductEditModel>(), CancellationToken.None), Times.Once);
            _productServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_ExistingProductValid_CallsUpdateAsync()
        {
            _viewModel.IsNew = false;
            _viewModel.Model.Name = "Milk";
            _viewModel.Model.ImageContent = [1];
            _viewModel.SelectedCategory = new ProductCategoryModel(Guid.NewGuid(), "Dairy");

            _productServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ProductEditModel>(), CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current.GoToAsync is called after a successful update; it throws a
            // NullReferenceException in this test host, which is caught internally.
            await _viewModel.SaveCommand.ExecuteAsync(null);

            _productServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ProductEditModel>(), CancellationToken.None), Times.Once);
            _productServiceMock.Verify(s => s.AddAsync(It.IsAny<ProductEditModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveAsync_ServiceReturnsFailure_SetsErrorMessage()
        {
            _viewModel.IsNew = true;
            _viewModel.Model.Name = "Milk";
            _viewModel.Model.ImageContent = [1];
            _viewModel.SelectedCategory = new ProductCategoryModel(Guid.NewGuid(), "Dairy");

            _productServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductEditModel>(), CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot save"));

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot save"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task SaveAsync_ServiceThrows_SetsErrorMessage()
        {
            _viewModel.IsNew = true;
            _viewModel.Model.Name = "Milk";
            _viewModel.Model.ImageContent = [1];
            _viewModel.SelectedCategory = new ProductCategoryModel(Guid.NewGuid(), "Dairy");

            _productServiceMock
                .Setup(s => s.AddAsync(It.IsAny<ProductEditModel>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task DeleteAsync_WhenIsNew_ReturnsWithoutCallingService()
        {
            _viewModel.IsNew = true;

            await _viewModel.DeleteCommand.ExecuteAsync(null);

            _productServiceMock.Verify(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
