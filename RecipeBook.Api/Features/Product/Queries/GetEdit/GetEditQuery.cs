using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetEdit
{
    public class GetEditQuery : IRequest<ProductEditModel>
    {
        public int Id { get; set; }
    }
}
