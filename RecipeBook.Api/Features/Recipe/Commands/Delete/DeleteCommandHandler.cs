using System.Net.Http.Headers;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using MealPlanner.Shared.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    public class DeleteCommandHandler(IRecipeRepository repository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly ILogger<DeleteCommandHandler> _logger = logger;
        private readonly IApiConfig _mealPlannerApiConfig = ServiceLocator.Current.GetInstance<MealPlannerApiConfig>();

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
                    var result = await client.GetFromJsonAsync<IList<MealPlanModel>>($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.MealPlan]}/search/{request.Id}", cancellationToken);
                    if (result != null && result.Any())
                    {
                        return new DeleteCommandResponse { Message = $"Recipe {itemToDelete.Name} can not be deleted, it is used in meal plans." };
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
