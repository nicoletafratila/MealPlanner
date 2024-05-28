using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public EditShopModel? Model { get; set; }
    }
}
