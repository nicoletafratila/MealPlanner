using MealPlanner.Api.Features.ProductCategory.Commands.AddProductCategory;
using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.ProductCategory.Commands.ProductCategory
{
    public class AddProductCategoryCommand : IRequest<AddProductCategoryCommandResponse>
    {
        public EditProductCategoryModel? Model { get; set; }
    }
}
