using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public EditRecipeCategoryModel? Model { get; set; }
    }
}
