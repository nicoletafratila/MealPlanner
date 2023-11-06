using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetRecipeCategory;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeCategoryController : ControllerBase
    {
        private readonly ISender _mediator;

        public RecipeCategoryController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IList<RecipeCategoryModel>> GetAll()
        {
            return await _mediator.Send(new GetRecipeCategoryQuery());
        }
    }
}
