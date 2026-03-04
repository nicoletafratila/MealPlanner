using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll
{
    /// <summary>
    /// Command to update multiple recipe categories at once.
    /// </summary>
    public class UpdateAllCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The list of recipe category models to update.
        /// </summary>
        public IList<RecipeCategoryModel>? Models { get; set; }

        public UpdateAllCommand()
        {
        }

        public UpdateAllCommand(IList<RecipeCategoryModel> models)
        {
            Models = models ?? throw new ArgumentNullException(nameof(models));
        }
    }
}