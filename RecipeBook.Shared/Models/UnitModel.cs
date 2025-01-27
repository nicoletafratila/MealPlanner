using Common.Constants.Units;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class UnitModel : BaseModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? UnitType { get; set; }
    }
}
