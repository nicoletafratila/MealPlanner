using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="Recipe"/> entities with eager-loading helpers.
    /// </summary>
    public class RecipeRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<Recipe, int>(dbContext), IRecipeRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public async Task<IReadOnlyList<Recipe>> GetAllByUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .Where(x => x.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<Recipe>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .ToListAsync(cancellationToken);
        }

        public override async Task<Recipe?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public override async Task UpdateAsync(Recipe entity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var existing = await Context.RecipeIngredients
                .Where(ri => ri.RecipeId == entity.Id)
                .ToListAsync(cancellationToken);

            var desired = entity.RecipeIngredients ?? [];
            var desiredProductIds = desired.Select(ri => ri.ProductId).ToHashSet();

            Context.RecipeIngredients.RemoveRange(
                existing.Where(ri => !desiredProductIds.Contains(ri.ProductId)));

            var existingByProduct = existing.ToDictionary(ri => ri.ProductId);
            var toAdd = new List<RecipeIngredient>();
            foreach (var item in desired)
            {
                if (existingByProduct.TryGetValue(item.ProductId, out var tracked))
                {
                    tracked.Quantity = item.Quantity;
                    tracked.UnitId = item.UnitId;
                }
                else
                {
                    toAdd.Add(new RecipeIngredient
                    {
                        RecipeId = entity.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitId = item.UnitId
                    });
                }
            }
            await Context.RecipeIngredients.AddRangeAsync(toAdd, cancellationToken);

            entity.RecipeIngredients = existing
                .Where(ri => desiredProductIds.Contains(ri.ProductId))
                .Concat(toAdd)
                .ToList();

            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Recipe?> GetByIdIncludeIngredientsAsync(
            int? id,
            CancellationToken cancellationToken)
        {
            if (id is null)
                return null;

            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)!.ThenInclude(p => p!.ProductCategory)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)!.ThenInclude(p => p!.BaseUnit)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Unit)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
        }

        public async Task<IReadOnlyList<Recipe>> SearchAsync(
            int categoryId,
            CancellationToken cancellationToken)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .Where(x => x.RecipeCategoryId == categoryId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Recipe?> SearchAsync(
            string name,
            string userId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .FirstOrDefaultAsync(
                    x => x.UserId == userId && x.Name != null && x.Name.ToLower() == name.ToLower(),
                    cancellationToken);
        }
    }
}