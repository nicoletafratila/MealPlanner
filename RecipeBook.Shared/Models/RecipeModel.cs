using System;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class RecipeModel : BaseModel
    {
        /// <summary>
        /// Database identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Recipe display name. Defaults to empty string to avoid null handling.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional image URL for this recipe.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Optional source (e.g., cookbook, URL, author).
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Navigation-model for the recipe category.
        /// </summary>
        public RecipeCategoryModel? RecipeCategory { get; set; }

        /// <summary>
        /// Cached / flattened category name (e.g., from a join).
        /// </summary>
        public string? RecipeCategoryName { get; set; }

        /// <summary>
        /// Cached / flattened category id (e.g., from a join or projection).
        /// </summary>
        public string? RecipeCategoryId { get; set; }

        /// <summary>
        /// Returns the most appropriate category name:
        /// 1) RecipeCategory.Name
        /// 2) RecipeCategoryName
        /// 3) empty string.
        /// </summary>
        public string EffectiveCategoryName =>
            RecipeCategory?.Name
            ?? RecipeCategoryName
            ?? string.Empty;

        public RecipeModel()
        {
        }

        public RecipeModel(int id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }
}