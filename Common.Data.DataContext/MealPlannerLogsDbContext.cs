using System.Reflection;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.DataContext
{
    public class MealPlannerLogsDbContext(DbContextOptions<MealPlannerLogsDbContext> options) : DbContext(options)
    {
        public DbSet<Log> Logs => Set<Log>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
