using Common.Data.DataContext;
using Common.Data.Repository;
using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace MealPlanner.Api.Repositories
{
    public class MealPlanRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<MealPlan, Guid>(dbContext), IMealPlanRepository
    {
        private MealPlannerDbContext Context =>
            DbContext as MealPlannerDbContext
            ?? throw new InvalidOperationException("DbContext is not MealPlannerDbContext.");

        public async Task<IReadOnlyList<MealPlan>> GetAllByUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await Context.MealPlans
                .Where(mp => mp.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public override async Task<MealPlan?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await Context.MealPlans!
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeCategory)
                .FirstOrDefaultAsync(mp => mp.Id == id, cancellationToken);
        }

        public override async Task DeleteAsync(MealPlan entity, CancellationToken cancellationToken)
        {
            await Context.MealPlanRecipes
                .Where(mpr => mpr.MealPlanId == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await Context.MealPlans
                .Where(mp => mp.Id == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public override async Task UpdateAsync(MealPlan entity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var desiredRecipeIds = entity.MealPlanRecipes?
                .Select(mpr => mpr.RecipeId)
                .ToHashSet() ?? [];

            var existingRecipes = await Context.MealPlanRecipes
                .Where(mpr => mpr.MealPlanId == entity.Id)
                .ToListAsync(cancellationToken);

            var existingRecipeIds = existingRecipes
                .Select(mpr => mpr.RecipeId)
                .ToHashSet();

            Context.MealPlanRecipes.RemoveRange(
                existingRecipes.Where(mpr => !desiredRecipeIds.Contains(mpr.RecipeId)));

            var newRecipes = desiredRecipeIds
                .Where(id => !existingRecipeIds.Contains(id))
                .Select(id => new MealPlanRecipe { RecipeId = id, MealPlanId = entity.Id })
                .ToList();
            await Context.MealPlanRecipes.AddRangeAsync(newRecipes, cancellationToken);

            // Restore navigation collection to already-tracked instances so that DetectChanges
            // (triggered by Entry) doesn't attempt to re-track the untracked AutoMapper objects,
            // which would cause an identity conflict on composite-key entities.
            entity.MealPlanRecipes = existingRecipes
                .Where(mpr => desiredRecipeIds.Contains(mpr.RecipeId))
                .Concat(newRecipes)
                .ToList();

            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task<MealPlan?> GetByIdIncludeRecipesAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await Context.MealPlans
                .AsNoTracking()
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeCategory)!
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeIngredients)!
                            .ThenInclude(ri => ri.Unit)
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeIngredients)!
                            .ThenInclude(ri => ri.Product)
                                .ThenInclude(p => p!.ProductCategory)
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeIngredients)!
                            .ThenInclude(ri => ri.Product)
                                .ThenInclude(p => p!.BaseUnit)
                .FirstOrDefaultAsync(mp => mp.Id == id, cancellationToken);
        }

        public async Task<IList<MealPlanRecipe>> SearchByRecipeCategoryIdsAsync(
            IList<Guid> categoryIds,
            string userId,
            CancellationToken cancellationToken)
        {
            if (categoryIds is null || categoryIds.Count == 0)
            {
                return [];
            }

            return await Context.MealPlanRecipes
                .AsNoTracking()
                .Include(x => x.Recipe)
                    .ThenInclude(r => r!.RecipeCategory)
                .Where(mpr => categoryIds.Contains(mpr.Recipe!.RecipeCategoryId)
                              && mpr.MealPlan!.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<KeyValuePair<Product, MealPlan>>> SearchByProductCategoryIdsAsync(
            IList<Guid> categoryIds,
            string userId,
            CancellationToken cancellationToken)
        {
            if (categoryIds is null || categoryIds.Count == 0)
            {
                return [];
            }

            var query =
                from ri in Context.RecipeIngredients.AsNoTracking()
                where ri.Product != null && categoryIds.Contains(ri.Product.ProductCategoryId)
                join mr in Context.MealPlanRecipes.AsNoTracking()
                    on ri.RecipeId equals mr.RecipeId
                where mr.MealPlan!.UserId == userId
                select new { ri.Product, mr.MealPlan };

            var results = await query.ToListAsync(cancellationToken);

            return results
                .Where(x => x.Product != null && x.MealPlan != null)
                .Select(x => new KeyValuePair<Product, MealPlan>(x.Product!, x.MealPlan!))
                .ToList();
        }

        public async Task<IList<MealPlan>> SearchByRecipeAsync(
            Guid recipeId,
            string userId,
            CancellationToken cancellationToken)
        {
            return await Context.MealPlanRecipes
                .AsNoTracking()
                .Where(mpr => mpr.RecipeId == recipeId && mpr.MealPlan!.UserId == userId)
                .Select(mpr => mpr.MealPlan!)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        public async Task<MealPlan?> SearchAsync(
            string name,
            string userId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return await Context.MealPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    mp => mp.UserId == userId && mp.Name != null && mp.Name.ToLower() == name.ToLower(),
                    cancellationToken);
        }
    }
}