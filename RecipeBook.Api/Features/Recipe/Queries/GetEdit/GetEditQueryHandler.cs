using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEdit
{
    /// <summary>
    /// Handles retrieving a recipe for editing.
    /// </summary>
    public class GetEditQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, RecipeEditModel>
    {
        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<RecipeEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdIncludeIngredientsAsync(request.Id);

            if (entity is null)
            {
                return new RecipeEditModel { Id = request.Id };
            }

            var model = _mapper.Map<RecipeEditModel>(entity);
            return model ?? new RecipeEditModel { Id = request.Id };
        }
    }
}