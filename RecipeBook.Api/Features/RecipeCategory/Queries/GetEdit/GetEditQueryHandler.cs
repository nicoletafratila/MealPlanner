using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit
{
    public class GetEditQueryHandler(IRecipeCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, RecipeCategoryEditModel>
    {
        private readonly IRecipeCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<RecipeCategoryEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<RecipeCategoryEditModel>(result);
        }
    }
}
