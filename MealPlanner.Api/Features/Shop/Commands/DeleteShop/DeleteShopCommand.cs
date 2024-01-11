using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.DeleteShop
{
    public class DeleteShopCommand : IRequest<DeleteShopCommandResponse>
    {
        public int Id { get; set; }
    }
}
