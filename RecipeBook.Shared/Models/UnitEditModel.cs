using System.ComponentModel.DataAnnotations;
using Common.Constants;
using Common.Shared.Models;

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
        [Range(0, 1)]
        public UnitType UnitType { get; set; }
    }
}
