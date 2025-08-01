using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit
{
    public class GetEditQueryHandler(IRecipeCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, RecipeCategoryEditModel>
    {
        public async Task<RecipeCategoryEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdAsync(request.Id);
            return mapper.Map<RecipeCategoryEditModel>(result);
        }
    }
}
