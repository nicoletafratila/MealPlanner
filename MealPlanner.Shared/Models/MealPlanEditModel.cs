﻿using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Validators;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    public class MealPlanEditModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The meal plan requires at least one recipe.")]
        public IList<RecipeModel>? Recipes { get; set; }
    }
}
