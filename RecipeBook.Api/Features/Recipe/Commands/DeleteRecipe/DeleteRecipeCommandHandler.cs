using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;
using MediatR;
using RecipeBook.Api.Repositories;
using System.Net.Http.Headers;

namespace RecipeBook.Api.Features.Recipe.Commands.DeleteRecipe
{
    public class DeleteRecipeCommandHandler : IRequestHandler<DeleteRecipeCommand, DeleteRecipeCommandResponse>
    {
        private readonly IRecipeRepository _repository;
        private readonly ILogger<DeleteRecipeCommandHandler> _logger;
        private readonly IApiConfig _mealPlannerApiConfig;

        public DeleteRecipeCommandHandler(IRecipeRepository repository, IServiceProvider serviceProvider, ILogger<DeleteRecipeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
            _mealPlannerApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);
        }

        public async Task<DeleteRecipeCommandResponse> Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteRecipeCommandResponse { Message = $"Could not find with id {request.Id}" };
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:7249/");//_mealPlannerApiConfig!.BaseUrl;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var result = await client.GetFromJsonAsync<IList<MealPlanModel>>($"{_mealPlannerApiConfig.Endpoints[ApiEndpointNames.MealPlanApi]}/search/{request.Id}");
                    if (result != null && result.Any())
                    {
                        return new DeleteRecipeCommandResponse { Message = "The recipe you try to delete is used in meal plans and cannot be deleted." };
                    }
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteRecipeCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteRecipeCommandResponse { Message = "An error occured when deleting the recipe." };
            }
        }
    }
}
