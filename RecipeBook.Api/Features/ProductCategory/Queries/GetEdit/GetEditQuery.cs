using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve a product category for editing.
    /// </summary>
    public class GetEditQuery : IRequest<ProductCategoryEditModel>
    {
        /// <summary>
        /// Id of the product category to edit.
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