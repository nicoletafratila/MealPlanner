using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IUnitService
    {
        Task<IEnumerable<UnitModel>> GetAll();
    }
}
