using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve a product for editing.
    /// </summary>
    public class GetEditQuery : IRequest<ProductEditModel>
    {
        /// <summary>
        /// Id of the product to edit.
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