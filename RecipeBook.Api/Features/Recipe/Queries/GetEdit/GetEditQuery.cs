using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEdit
{
    public class GetEditQuery : IRequest<EditRecipeModel>
    {
        public int Id { get; set; }
    }
}
