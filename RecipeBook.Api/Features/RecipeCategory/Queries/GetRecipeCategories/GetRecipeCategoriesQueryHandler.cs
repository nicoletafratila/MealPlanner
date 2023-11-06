using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetRecipeCategories
{
    public class GetRecipeCategoriesQueryHandler : IRequestHandler<GetRecipeCategoriesQuery, IList<RecipeCategoryModel>>
    {
        private readonly IRecipeCategoryRepository _repository;
        private readonly IMapper _mapper;

        public GetRecipeCategoriesQueryHandler(IRecipeCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IList<RecipeCategoryModel>> Handle(GetRecipeCategoriesQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<RecipeCategoryModel>>(results).OrderBy(r => r.DisplaySequence).ToList();
        }
    }
}
