using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit
{
    /// <summary>
    /// Handles retrieving a recipe category for editing.
    /// </summary>
    public class GetEditQueryHandler(IRecipeCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, RecipeCategoryEditModel>
    {
        private readonly IRecipeCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<RecipeCategoryEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
            {
                return new RecipeCategoryEditModel { Id = request.Id };
            }

            var model = _mapper.Map<RecipeCategoryEditModel>(entity);
            return model ?? new RecipeCategoryEditModel { Id = request.Id };
        }
    }
}