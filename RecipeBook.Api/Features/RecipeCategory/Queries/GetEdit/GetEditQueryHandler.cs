using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit
{
    public class GetEditQueryHandler(IRecipeCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, EditRecipeCategoryModel>
    {
        private readonly IRecipeCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditRecipeCategoryModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<EditRecipeCategoryModel>(result);
        }
    }
}
