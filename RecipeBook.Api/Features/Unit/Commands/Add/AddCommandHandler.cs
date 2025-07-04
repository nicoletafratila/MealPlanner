using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Unit.Commands.Add
{
    public class AddCommandHandler(IUnitRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, AddCommandResponse>
    {
        private readonly IUnitRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddCommandHandler> _logger = logger;

        public async Task<AddCommandResponse> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var Units = await _repository.GetAllAsync();
                var existingItem = Units?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return new AddCommandResponse { Id = 0, Message = "This product category already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.Unit>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddCommandResponse { Message = "An error occurred when saving the product category." };
            }
        }
    }
}
