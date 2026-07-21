using Common.Models;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace Common.Core.Tests
{
    [TestFixture]
    public class ValidationBehaviorTests
    {
        public class TestRequest : IRequest<CommandResponse>
        {
        }

        public class TestNonCommandRequest : IRequest<string>
        {
        }

        [Test]
        public async Task Handle_NoValidators_CallsNext()
        {
            var behavior = new ValidationBehavior<TestRequest, CommandResponse>([]);
            var next = new Mock<RequestHandlerDelegate<CommandResponse>>(MockBehavior.Strict);
            var expected = CommandResponse.Success();
            next.Setup(n => n()).ReturnsAsync(expected);

            var result = await behavior.Handle(new TestRequest(), next.Object, CancellationToken.None);

            Assert.That(result, Is.SameAs(expected));
        }

        [Test]
        public async Task Handle_ResponseTypeIsNotCommandResponse_SkipsValidationAndCallsNext()
        {
            var validator = new Mock<IValidator<TestNonCommandRequest>>(MockBehavior.Strict);
            var behavior = new ValidationBehavior<TestNonCommandRequest, string>([validator.Object]);
            var next = new Mock<RequestHandlerDelegate<string>>(MockBehavior.Strict);
            next.Setup(n => n()).ReturnsAsync("value");

            var result = await behavior.Handle(new TestNonCommandRequest(), next.Object, CancellationToken.None);

            Assert.That(result, Is.EqualTo("value"));
            validator.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_ValidationFails_ReturnsFailedCommandResponseWithoutCallingNext()
        {
            var validator = new Mock<IValidator<TestRequest>>(MockBehavior.Strict);
            validator.Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult([new ValidationFailure("Field", "Field is required.")]));
            var behavior = new ValidationBehavior<TestRequest, CommandResponse>([validator.Object]);
            var next = new Mock<RequestHandlerDelegate<CommandResponse>>(MockBehavior.Strict);

            var result = await behavior.Handle(new TestRequest(), next.Object, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Field is required."));
            }
            next.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_ValidationSucceeds_CallsNext()
        {
            var validator = new Mock<IValidator<TestRequest>>(MockBehavior.Strict);
            validator.Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            var behavior = new ValidationBehavior<TestRequest, CommandResponse>([validator.Object]);
            var next = new Mock<RequestHandlerDelegate<CommandResponse>>(MockBehavior.Strict);
            var expected = CommandResponse.Success();
            next.Setup(n => n()).ReturnsAsync(expected);

            var result = await behavior.Handle(new TestRequest(), next.Object, CancellationToken.None);

            Assert.That(result, Is.SameAs(expected));
        }
    }
}
