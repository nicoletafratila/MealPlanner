using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Delete
{
    public class DeleteCommand : IRequest<CommandResponse>
    {
        public int Id { get; set; }
    }
}
