﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class ShoppingListProduct
    {
        public decimal Quantity { get; set; }
        public bool Collected { get; set; }
        public int DisplaySequence { get; set; }

        [ForeignKey("ShoppingListId")]
        public ShoppingList? ShoppingList { get; set; }
        public int ShoppingListId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int ProductId { get; set; }
    }
}
