using Microsoft.EntityFrameworkCore;

namespace Svc_Two.Context
{
    public sealed class SvcDbContext : DbContext
    {
        public SvcDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

    }
}