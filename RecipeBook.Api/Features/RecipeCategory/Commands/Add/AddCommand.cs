using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Add
{
    public class AddCommand : IRequest<CommandResponse?>
    {
        public RecipeCategoryEditModel? Model { get; set; }
    }
}
