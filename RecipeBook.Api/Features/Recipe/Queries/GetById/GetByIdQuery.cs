using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetById
{
    public class GetByIdQuery : IRequest<RecipeModel>
    {
        public int Id { get; set; }
    }
}
