using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetEdit
{ 
    public class GetEditQuery : IRequest<EditProductCategoryModel>
    {
        public int Id { get; set; }
    }
}
