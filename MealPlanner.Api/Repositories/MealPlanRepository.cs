using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Common.Pagination;
using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace MealPlanner.Api.Repositories
{
    public class MealPlanRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<MealPlan, Guid>(dbContext), IMealPlanRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

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

            var existing = await Context.MealPlanRecipes
                .Where(mpr => mpr.MealPlanId == entity.Id)
                .ToListAsync(cancellationToken);
            Context.MealPlanRecipes.RemoveRange(existing);

            var newRecipes = entity.MealPlanRecipes?
                .Select(mpr => new MealPlanRecipe { RecipeId = mpr.RecipeId, MealPlanId = entity.Id })
                .ToList() ?? [];

            await Context.MealPlanRecipes.AddRangeAsync(newRecipes, cancellationToken);

            entity.MealPlanRecipes = newRecipes;
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

        public async Task<IList<CategoryItemCount>> SearchByRecipeCategoryIdsAsync(
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
                .Where(mpr => categoryIds.Contains(mpr.Recipe!.RecipeCategoryId)
                              && mpr.MealPlan!.UserId == userId)
                .GroupBy(mpr => new { mpr.Recipe!.RecipeCategoryId, mpr.Recipe!.Name })
                .Select(g => new CategoryItemCount(g.Key.RecipeCategoryId, g.Key.Name ?? string.Empty, g.Count()))
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CategoryItemCount>> SearchByProductCategoryIdsAsync(
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
                group ri by new { ri.Product!.ProductCategoryId, ri.Product.Name } into g
                select new CategoryItemCount(g.Key.ProductCategoryId, g.Key.Name!, g.Count());

            return await query.ToListAsync(cancellationToken);
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

        public async Task<PagedQueryResult<MealPlan>> SearchByUserAsync(
            string userId,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            IQueryable<MealPlan> query = Context.MealPlans
                .Where(mp => mp.UserId == userId)
                .OrderBy(mp => mp.Id);

            var filtered = query.ApplyFilters(filters).ApplySorting(sorting);

            return await filtered.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}