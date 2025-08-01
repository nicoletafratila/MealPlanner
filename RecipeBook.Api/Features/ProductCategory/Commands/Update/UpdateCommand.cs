using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Update
{
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        public ProductCategoryEditModel? Model { get; set; }
    }
}
