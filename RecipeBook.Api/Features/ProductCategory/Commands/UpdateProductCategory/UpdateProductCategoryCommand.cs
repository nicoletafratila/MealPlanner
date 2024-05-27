using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.ProductCategory.Commands.UpdateProductCategory
{
    public class UpdateProductCategoryCommand : IRequest<UpdateProductCategoryCommandResponse>
    {
        public EditProductCategoryModel? Model { get; set; }
    }
}
