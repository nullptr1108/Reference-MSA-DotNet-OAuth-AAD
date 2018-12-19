using Microsoft.EntityFrameworkCore;

namespace Svc_C.Context
{
    public sealed class SvcDbContext : DbContext
    {
        public SvcDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

    }
}