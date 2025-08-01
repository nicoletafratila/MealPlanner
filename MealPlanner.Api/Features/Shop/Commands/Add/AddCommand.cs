using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Add
{
    public class AddCommand : IRequest<CommandResponse?>
    {
        public ShopEditModel? Model { get; set; }
    }
}
