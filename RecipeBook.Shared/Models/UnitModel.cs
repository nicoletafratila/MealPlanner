using System;
using Common.Constants.Units;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Read-only/unit lookup model used across the UI and services.
    /// </summary>
    public sealed class UnitModel : BaseModel
    {
        /// <summary>
        /// Database identity for the unit.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display name of the unit (e.g., "Kilogram", "Liter").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Category/type of the unit (e.g., Weight, Volume).
        /// </summary>
        public UnitType UnitType { get; set; }

        public UnitModel()
        {
        }

        public UnitModel(int id, string name, UnitType unitType)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            UnitType = unitType;
        }

        /// <summary>
        /// Returns the unit name, useful for debugging and UI binding.
        /// </summary>
        public override string ToString() => Name;
    }
}