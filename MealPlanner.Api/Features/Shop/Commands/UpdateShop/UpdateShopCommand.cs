using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.UpdateShop
{
    public class UpdateShopCommand : IRequest<UpdateShopCommandResponse>
    {
        public EditShopModel? Model { get; set; }
    }
}
