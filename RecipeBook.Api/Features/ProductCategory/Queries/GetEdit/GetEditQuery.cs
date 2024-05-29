using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetEdit
{ 
    public class GetEditQuery : IRequest<ProductCategoryEditModel>
    {
        public int Id { get; set; }
    }
}
