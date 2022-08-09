using Microsoft.EntityFrameworkCore;
using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecipeRepository> _logger;

        public RecipeRepository(AppDbContext context, ILogger<RecipeRepository> logger)
        {
            _context = context;
            _logger = logger;
            context.Recipes.Include(x => x.RecipeIngredients).ThenInclude(x => x.Ingredient).ToList();
        }

        public async Task<IEnumerable<Recipe>> GetAllAsync()
        {
            _logger.LogInformation($"Getting all recipes");
            return await _context.Recipes.ToListAsync();
        }

        public async Task<Recipe> GetAsync(int id)
        {
            _logger.LogInformation($"Getting a recipe for {id}");
            return await _context.Recipes.Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public void Add(Recipe entity)
        {
            _logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");
            _context.Add(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
