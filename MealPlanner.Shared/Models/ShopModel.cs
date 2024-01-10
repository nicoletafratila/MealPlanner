﻿namespace MealPlanner.Shared.Models
{
    public class ShopModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public IList<ShopDisplaySequenceModel>? DisplaySequence { get; set; }
    }
}
