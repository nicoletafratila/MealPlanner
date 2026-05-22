using Common.Models;
using Identity.Api.Controllers;
using Identity.Api.Features.ContactUs.Commands.Send;
using Identity.Shared.Models;
using MediatR;
using Moq;

namespace Identity.Api.Tests.Controllers
{
    [TestFixture]
    public class ContactUsControllerTests
    {
        private Mock<ISender> _mediatorMock = null!;
        private ContactUsController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new ContactUsController(_mediatorMock.Object);
        }

        [Test]
        public void Ctor_NullMediator_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactUsController(null!));
        }

        [Test]
        public async Task SendAsync_ForwardsModelToMediator_AndReturnsResult()
        {
            var model = new ContactUsModel
            {
                Name = "Jane",
                EmailAddress = "jane@example.com",
                Subject = "Hello",
                Message = "Body"
            };

            var expected = CommandResponse.Success("Sent");

            _mediatorMock
                .Setup(m => m.Send(It.Is<SendContactUsCommand>(c => c.Model == model), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.SendAsync(model, CancellationToken.None);

            Assert.That(result, Is.SameAs(expected));
            _mediatorMock.Verify(
                m => m.Send(It.Is<SendContactUsCommand>(c => c.Model == model), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SendAsync_MediatorReturnsNull_ReturnsNull()
        {
            var model = new ContactUsModel { Name = "x", EmailAddress = "x@x.com", Subject = "s", Message = "m" };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SendContactUsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CommandResponse?)null);

            var result = await _controller.SendAsync(model, CancellationToken.None);

            Assert.That(result, Is.Null);
        }
    }
}
