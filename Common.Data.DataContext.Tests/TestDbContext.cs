using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Common.Data.DataContext.Tests
{
    public class TestDbContext(DatabaseFacade database, DbContextOptions options) : DbContext(options)
    {
        private readonly DatabaseFacade _database = database;
        
        public override DatabaseFacade Database => _database;
    }
}
