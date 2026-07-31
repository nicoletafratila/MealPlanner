using Common.Constants.Units;
using Common.Models;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class UnitEditViewModelTests
    {
        private Mock<IUnitService> _unitServiceMock = null!;
        private UnitEditViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _unitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _viewModel = new UnitEditViewModel(_unitServiceMock.Object);
        }

        [Test]
        public void OnUnitIdChanged_WithEmptyGuid_SetsIsNewTrueAndDoesNotLoad()
        {
            _viewModel.UnitId = Guid.Empty.ToString();

            Assert.That(_viewModel.IsNew, Is.True);

            _unitServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public void OnUnitIdChanged_WithNonEmptyGuid_SetsIsNewFalseAndLoadsModel()
        {
            var id = Guid.NewGuid();
            var existing = new UnitEditModel(id, "Kilogram", UnitType.Weight);

            _unitServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _viewModel.UnitId = id.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.False);
                Assert.That(_viewModel.Model.Id, Is.EqualTo(id));
                Assert.That(_viewModel.Model.Name, Is.EqualTo("Kilogram"));
                Assert.That(_viewModel.Model.UnitType, Is.EqualTo(UnitType.Weight));
            }
        }

        [Test]
        public async Task SaveAsync_NameMissing_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = string.Empty;
            _viewModel.Model.UnitType = UnitType.Weight;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.UnitNameRequired));
                Assert.That(_viewModel.IsBusy, Is.False);
            }

            _unitServiceMock.Verify(
                s => s.AddAsync(It.IsAny<UnitEditModel>(), CancellationToken.None),
                Times.Never);
            _unitServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<UnitEditModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_UnitTypeOutOfRange_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "Kilogram";
            _viewModel.Model.UnitType = (UnitType)4;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.UnitTypeRange));
                Assert.That(_viewModel.IsBusy, Is.False);
            }

            _unitServiceMock.Verify(
                s => s.AddAsync(It.IsAny<UnitEditModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_UnitTypeNegative_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "Kilogram";
            _viewModel.Model.UnitType = (UnitType)(-1);

            await _viewModel.SaveCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.UnitTypeRange));

            _unitServiceMock.Verify(
                s => s.AddAsync(It.IsAny<UnitEditModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_UnitTypeAtValidBoundary_CallsService()
        {
            _viewModel.UnitId = Guid.Empty.ToString();
            _viewModel.Model.Name = "Piece";
            _viewModel.Model.UnitType = (UnitType)3;

            _unitServiceMock
                .Setup(s => s.AddAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current.GoToAsync runs after a successful save inside the try/catch, so the
            // resulting NullReferenceException in this test host is swallowed into ErrorMessage.
            // Only the service call is verified here.
            await _viewModel.SaveCommand.ExecuteAsync(null);

            _unitServiceMock.Verify(s => s.AddAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveAsync_NewUnitValid_CallsAddAsync()
        {
            _viewModel.UnitId = Guid.Empty.ToString();
            _viewModel.Model.Name = "Kilogram";
            _viewModel.Model.UnitType = UnitType.Weight;

            _unitServiceMock
                .Setup(s => s.AddAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.SaveCommand.ExecuteAsync(null);

            _unitServiceMock.Verify(s => s.AddAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveAsync_ExistingUnitValid_CallsUpdateAsync()
        {
            var id = Guid.NewGuid();
            var existing = new UnitEditModel(id, "Kilogram", UnitType.Weight);
            _unitServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);
            _viewModel.UnitId = id.ToString();

            _viewModel.Model.Name = "Kilogram updated";
            _viewModel.Model.UnitType = UnitType.Weight;

            _unitServiceMock
                .Setup(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.SaveCommand.ExecuteAsync(null);

            _unitServiceMock.Verify(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_WhenIsNew_ReturnsWithoutCallingService()
        {
            // DeleteAsync confirms via Shell.Current.DisplayAlertAsync before any try/catch, so
            // calling it past the IsNew guard would throw in this test host. Only the guard-clause
            // return path is exercised here.
            _viewModel.UnitId = Guid.Empty.ToString();
            Assert.That(_viewModel.IsNew, Is.True);

            await _viewModel.DeleteCommand.ExecuteAsync(null);

            _unitServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }
    }
}
