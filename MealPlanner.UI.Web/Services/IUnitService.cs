using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IUnitService
    {
        Task<IList<UnitModel>> GetAll();
    }
}
