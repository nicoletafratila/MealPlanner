using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public RecipeCategoryEditModel? Model { get; set; }
    }
}
