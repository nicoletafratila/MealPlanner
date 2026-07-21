using Common.Models;
using FluentValidation;
using MediatR;

namespace Common.Core
{
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
                return await next();

            var failures = new List<FluentValidation.Results.ValidationFailure>();
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(request, cancellationToken);
                failures.AddRange(result.Errors);
            }

            if (failures.Count == 0)
                return await next();

            if (typeof(TResponse) == typeof(CommandResponse))
                return (TResponse)(object)CommandResponse.Failed(failures[0].ErrorMessage);

            throw new ValidationException(failures);
        }
    }
}
