using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEdit
{
    public class GetEditQuery : IRequest<RecipeEditModel>
    {
        public int Id { get; set; }
    }
}
