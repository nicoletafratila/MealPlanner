﻿using Common.Shared;

namespace RecipeBook.Shared.Models
{
    public class RecipeCategoryModel : BaseModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int DisplaySequence { get; set; }
    }
}
