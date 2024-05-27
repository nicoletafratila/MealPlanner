using MediatR;

namespace MealPlanner.Api.Features.ProductCategory.Commands.DeleteProductCategory
{
    public class DeleteProductCategoryCommand : IRequest<DeleteProductCategoryCommandResponse>
    {
        public int Id { get; set; }
    }
}
