using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Add
{
    /// <summary>
    /// Handles adding a new product category.
    /// </summary>
    public class AddCommandHandler(
        IProductCategoryRepository repository,
        IMapper mapper,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IProductCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), "Model cannot be null.");

            try
            {
                var allCategories = await _repository.GetAllAsync(cancellationToken) ?? [];

                var newName = request.Model.Name?.Trim();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    var exists = allCategories.Any(c =>
                        !string.IsNullOrWhiteSpace(c.Name) &&
                        c.Name.Trim().Equals(newName, StringComparison.OrdinalIgnoreCase));

                    if (exists)
                    {
                        return CommandResponse.Failed("This product category already exists.");
                    }
                }

                var mapped = _mapper.Map<Common.Data.Entities.ProductCategory>(request.Model);
                await _repository.AddAsync(mapped, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when saving the product category '{Name}'.", request.Model.Name);
                return CommandResponse.Failed("An error occurred when saving the product category.");
            }
        }
    }
}