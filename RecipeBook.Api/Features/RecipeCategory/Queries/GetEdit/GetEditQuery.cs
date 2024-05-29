using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit
{ 
    public class GetEditQuery : IRequest<RecipeCategoryEditModel>
    {
        public int Id { get; set; }
    }
}
