using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class RecipeModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? ImageUrl { get; set; }

        public string Category { get; set; }
        public int DisplaySequence { get; set; }
    }
}
