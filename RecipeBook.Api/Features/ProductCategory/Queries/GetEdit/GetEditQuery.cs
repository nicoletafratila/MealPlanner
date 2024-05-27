using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.ProductCategory.Queries.GetEdit
{ 
    public class GetEditQuery : IRequest<EditProductCategoryModel>
    {
        public int Id { get; set; }
    }
}
