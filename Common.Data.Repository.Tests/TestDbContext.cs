using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository.Tests
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }


    public class TestEntity : Entity<int>
    {
        public string Name { get; set; } = string.Empty;
    }
}
