using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll
{
    public class UpdateAllCommand : IRequest<UpdateAllCommandResponse>
    {
        public IList<RecipeCategoryModel>? Models { get; set; }
    }
}
