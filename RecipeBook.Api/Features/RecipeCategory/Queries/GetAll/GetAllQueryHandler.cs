using AutoMapper;
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
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<RecipeCategoryModel>>(results).OrderBy(r => r.DisplaySequence).ToList();
        }
    }
}
