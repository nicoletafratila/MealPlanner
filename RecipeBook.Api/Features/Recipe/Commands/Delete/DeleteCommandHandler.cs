using System.Net.Http.Headers;
using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    public class DeleteCommandHandler(IRecipeRepository repository, IServiceProvider serviceProvider, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly ILogger<DeleteCommandHandler> _logger = logger;
        private readonly IApiConfig _mealPlannerApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);

        public async Task<DeleteCommandResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteCommandResponse { Message = $"Could not find with id {request.Id}" };
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = _mealPlannerApiConfig?.BaseUrl;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var result = await client.GetFromJsonAsync<IList<MealPlanModel>>($"{_mealPlannerApiConfig?.Endpoints![ApiEndpointNames.MealPlanApi]}/search/{request.Id}", cancellationToken);
                    if (result != null && result.Any())
                    {
                        return new DeleteCommandResponse { Message = "The recipe you try to delete is used in meal plans and cannot be deleted." };
                    }
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteCommandResponse { Message = "An error occured when deleting the recipe." };
            }
        }
    }
}
