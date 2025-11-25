using System.Net.Http.Headers;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    public class DeleteCommandHandler(IRecipeRepository repository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse>
    {
        private readonly IApiConfig mealPlannerApiConfig = ServiceLocator.Current.GetInstance<MealPlannerApiConfig>();

        public async Task<CommandResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}");
                }

                using (var client = new HttpClient())
                {
                    client.EnsureAuthorizationHeader(request.AuthToken);
                    client.BaseAddress = mealPlannerApiConfig?.BaseUrl;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var result = await client.GetFromJsonAsync<IList<MealPlanModel>>($"{mealPlannerApiConfig?.Controllers![MealPlannerControllers.MealPlan]}/searchbyid?id={request.Id}", cancellationToken);
                    if (result != null && result.Any())
                    {
                        return CommandResponse.Failed($"Recipe {itemToDelete.Name} can not be deleted, it is used in meal plans.");
                    }
                }

                await repository.DeleteAsync(itemToDelete!);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when deleting the recipe.");
            }
        }
    }
}
