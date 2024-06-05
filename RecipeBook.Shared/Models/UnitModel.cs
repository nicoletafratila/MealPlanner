using Common.Constants;
using Common.Shared.Models;

namespace RecipeBook.Shared.Models
{
    public class UnitModel : BaseModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public UnitType UnitType { get; set; }
    }
}
