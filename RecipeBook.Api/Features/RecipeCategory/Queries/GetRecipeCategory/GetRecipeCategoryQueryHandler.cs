using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetRecipeCategory
{
    public class GetRecipeCategoryQueryHandler : IRequestHandler<GetRecipeCategoryQuery, IList<RecipeCategoryModel>>
    {
        private readonly IRecipeCategoryRepository _repository;
        private readonly IMapper _mapper;

        public GetRecipeCategoryQueryHandler(IRecipeCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IList<RecipeCategoryModel>> Handle(GetRecipeCategoryQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<RecipeCategoryModel>>(results).OrderBy(r => r.DisplaySequence).ToList();
        }
    }
}
