using Common.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Common.Core
{
    public class ValidationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not ValidationException validationException)
                return;

            var message = validationException.Errors.FirstOrDefault()?.ErrorMessage ?? validationException.Message;
            context.Result = new BadRequestObjectResult(CommandResponse.Failed(message));
            context.ExceptionHandled = true;
        }
    }
}
