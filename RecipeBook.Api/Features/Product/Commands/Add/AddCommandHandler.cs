using AutoMapper;
using Common.Api;
using Common.Models;
using MediatR;
using RecipeBook.Api.Features.Product.Resources;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.Add
{
    /// <summary>
    /// Handles adding a new product.
    /// </summary>
    public class AddCommandHandler(
        IProductRepository repository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IProductRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), ProductMessages.ModelCannotBeNull);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return CommandResponse.Failed(ProductMessages.UserIdRequired);

                var existingItem = await _repository.SearchAsync(request.Model.Name!, userId, cancellationToken);
                if (existingItem is not null)
                    return CommandResponse.Failed(ProductMessages.ProductAlreadyExists);

                var mapped = _mapper.Map<Common.Data.Entities.Product>(request.Model);
                mapped.UserId = userId;
                await _repository.AddAsync(mapped, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when saving the product with name {Name}.", request.Model.Name);
                return CommandResponse.Failed(ProductMessages.SaveFailed);
            }
        }
    }
}