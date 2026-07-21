using Common.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Common.Core.Tests
{
    [TestFixture]
    public class ValidationExceptionFilterTests
    {
        private static ExceptionContext CreateContext(Exception exception)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor());

            return new ExceptionContext(actionContext, [])
            {
                Exception = exception
            };
        }

        [Test]
        public void OnException_ValidationException_ReturnsBadRequestWithFailedCommandResponse()
        {
            var context = CreateContext(new ValidationException([new ValidationFailure("Field", "Field is required.")]));
            var filter = new ValidationExceptionFilter();

            filter.OnException(context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(context.ExceptionHandled, Is.True);
                Assert.That(context.Result, Is.TypeOf<BadRequestObjectResult>());
            }
            var response = (CommandResponse)((BadRequestObjectResult)context.Result!).Value!;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.Succeeded, Is.False);
                Assert.That(response.Message, Is.EqualTo("Field is required."));
            }
        }

        [Test]
        public void OnException_OtherException_LeavesExceptionUnhandled()
        {
            var context = CreateContext(new InvalidOperationException("boom"));
            var filter = new ValidationExceptionFilter();

            filter.OnException(context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(context.ExceptionHandled, Is.False);
                Assert.That(context.Result, Is.Null);
            }
        }
    }
}
