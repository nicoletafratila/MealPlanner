using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve a recipe category for editing.
    /// </summary>
    public class GetEditQuery : IRequest<RecipeCategoryEditModel>
    {
        /// <summary>
        /// Id of the recipe category to edit.
        /// </summary>
        public int Id { get; set; }

        public GetEditQuery()
        {
        }

        public GetEditQuery(int id)
        {
            Id = id;
        }
    }
}