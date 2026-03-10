using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetById
{
    /// <summary>
    /// Handles retrieving a recipe by id (read-only model).
    /// </summary>
    public class GetByIdQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<GetByIdQuery, RecipeModel>
    {
        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<RecipeModel> Handle(GetByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
            {
                return new RecipeModel { Id = request.Id };
            }

            var model = _mapper.Map<RecipeModel>(entity);
            return model ?? new RecipeModel { Id = request.Id };
        }
    }
}