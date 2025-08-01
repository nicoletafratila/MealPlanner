using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Update
{
    public class UpdateCommand : IRequest<CommandResponse>
    {
        public ShopEditModel? Model { get; set; }
    }
}
