using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.ProductCategory.Queries.GetEditProductCategory
{
    public class GetEditProductCategoryQuery : IRequest<EditProductCategoryModel>
    {
        public int Id { get; set; }
    }
}
