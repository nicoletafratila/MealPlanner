using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetAll
{
    public class GetAllQueryHandler(IRecipeCategoryRepository repository, IMapper mapper) : IRequestHandler<GetAllQuery, IList<RecipeCategoryModel>>
    {
        private readonly IRecipeCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<RecipeCategoryModel>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<RecipeCategoryModel>>(data).OrderBy(r => r.DisplaySequence).ToList();
            results.SetIndexes();
            return results;
        }
    }
}
