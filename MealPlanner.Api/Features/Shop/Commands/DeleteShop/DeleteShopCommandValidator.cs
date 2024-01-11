using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Commands.DeleteShop
{
    public class DeleteShopCommandValidator : AbstractValidator<DeleteShopCommand>
    {
        public DeleteShopCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
