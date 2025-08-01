using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Add
{
    public class AddCommand : IRequest<CommandResponse?>
    {
        public ProductCategoryEditModel? Model { get; set; }
    }
}
