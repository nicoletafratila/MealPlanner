using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Unit.Commands.Add
{
    /// <summary>
    /// Handles adding new units.
    /// </summary>
    public class AddCommandHandler(
        IUnitRepository repository,
        IMapper mapper,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IUnitRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), "Model cannot be null.");

            try
            {
                var existingUnits = await _repository.GetAllAsync() ?? [];

                var newName = request.Model.Name?.Trim();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    var duplicate = existingUnits
                        .FirstOrDefault(u =>
                            !string.IsNullOrWhiteSpace(u.Name) &&
                            u.Name.Trim().Equals(newName, StringComparison.OrdinalIgnoreCase));

                    if (duplicate != null)
                    {
                        return CommandResponse.Failed("This product category already exists.");
                    }
                }

                var mapped = _mapper.Map<Common.Data.Entities.Unit>(request.Model);
                await _repository.AddAsync(mapped);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when adding a unit.");
                return CommandResponse.Failed("An error occurred when saving the product category.");
            }
        }
    }
}