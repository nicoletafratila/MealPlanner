using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.ProductCategory.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public EditProductCategoryModel? Model { get; set; }
    }
}
