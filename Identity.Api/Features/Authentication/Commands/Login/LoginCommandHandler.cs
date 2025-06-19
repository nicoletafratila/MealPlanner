using AutoMapper;
using Common.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler(SignInManager<ApplicationUser> signInManager, IMapper mapper, ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<LoginCommandHandler> _logger = logger;

        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            //try
            //{
            //    var existingItem = await _repository.SearchAsync(request.Model?.Name!);
            //    if (existingItem != null)
            //        return new LoginCommandResponse { Id = 0, Message = "This meal plan already exists." };

            //    var mapped = _mapper.Map<Common.Data.Entities.MealPlan>(request.Model);
            //    var newItem = await _repository.AddAsync(mapped);
            //    return new LoginCommandResponse { Id = newItem.Id, Message = string.Empty };
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message, ex);
            //    return new LoginCommandResponse { Message = "An error occured when saving the meal plan." };
            //}

            var result = await signInManager.PasswordSignInAsync(request.Model?.Username!, request.Model?.Password!, true, true);

            return new LoginCommandResponse();
        }
    }
}


