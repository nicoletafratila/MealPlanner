using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Update
{
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        public RecipeCategoryEditModel? Model { get; set; }
    }
}
