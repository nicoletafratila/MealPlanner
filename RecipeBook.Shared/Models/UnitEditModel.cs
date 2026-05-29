using System.ComponentModel.DataAnnotations;using Common.Constants.Units; using Common.Models; using RecipeBook.Shared.Resources;

namespace RecipeBook.Shared.Models
{
    public class UnitEditModel : BaseModel
    {
        /// <summary>
        /// Database identity or zero for new entities.
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Display name of the unit (e.g., "Kilogram", "Liter").
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The category/type of this unit (Weight, Volume, etc.).
        /// </summary>
        [Required]
        [Range(0, 3, ErrorMessageResourceName = nameof(RecipeBookSharedMessages.UnitTypeRange), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        public UnitType UnitType { get; set; }

        public UnitEditModel()
        {
        }

        public UnitEditModel(int id, string name, UnitType unitType)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            UnitType = unitType;
        }

        public override string ToString() => $"{Name} ({UnitType})";
    }
}