using MealPlanner.UI.Mobile.ViewModels;

namespace MealPlanner.UI.Mobile.Tests.ViewModels
{
    [TestFixture]
    public class BaseViewModelTests
    {
        private sealed class TestViewModel : BaseViewModel
        {
            public void InvokeSetError(string? message) => SetError(message);
            public void InvokeSetSuccess(string? message) => SetSuccess(message);
            public void InvokeClearMessages() => ClearMessages();
        }

        private TestViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _viewModel = new TestViewModel();
        }

        [Test]
        public void SetError_SetsErrorMessage_ClearsSuccessMessage()
        {
            _viewModel.InvokeSetSuccess("previous success");

            _viewModel.InvokeSetError("something failed");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("something failed"));
                Assert.That(_viewModel.SuccessMessage, Is.Null);
            }
        }

        [Test]
        public void SetSuccess_SetsSuccessMessage_ClearsErrorMessage()
        {
            _viewModel.InvokeSetError("previous error");

            _viewModel.InvokeSetSuccess("it worked");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.SuccessMessage, Is.EqualTo("it worked"));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public void ClearMessages_ClearsBothErrorAndSuccessMessages()
        {
            _viewModel.InvokeSetError("error");
            _viewModel.InvokeSetSuccess("success");

            _viewModel.InvokeClearMessages();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.SuccessMessage, Is.Null);
            }
        }

        [Test]
        public void IsNotBusy_FollowsInverseOfIsBusy()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsBusy, Is.False);
                Assert.That(_viewModel.IsNotBusy, Is.True);
            }

            _viewModel.IsBusy = true;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsBusy, Is.True);
                Assert.That(_viewModel.IsNotBusy, Is.False);
            }
        }
    }
}
