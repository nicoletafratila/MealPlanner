using System.ComponentModel.DataAnnotations;
using Common.Constants.Units;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class UnitEditModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [Range(0, 3)]
        public UnitType UnitType { get; set; }
    }
}
