using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Common.Data.DataContext.Tests
{
    public class FakeDatabaseFacade(DbContext context, bool canConnect) : DatabaseFacade(context)
    {
        public override bool CanConnect()
        {
            return canConnect;
        }
    }
}
