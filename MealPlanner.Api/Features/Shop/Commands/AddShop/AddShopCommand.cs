using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.AddShop
{
    public class AddShopCommand : IRequest<AddShopCommandResponse>
    {
        public EditShopModel? Model { get; set; }
    }
}
