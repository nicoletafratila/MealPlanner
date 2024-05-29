using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public ProductCategoryEditModel? Model { get; set; }
    }
}
