using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.ProductCategory.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public EditProductCategoryModel? Model { get; set; }
    }
}
