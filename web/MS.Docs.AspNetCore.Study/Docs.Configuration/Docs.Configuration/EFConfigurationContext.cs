using Microsoft.EntityFrameworkCore;

namespace Docs.Configuration
{
    public class EFConfigurationContext : DbContext
    {
        public EFConfigurationContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EFConfigurationValue> Values { get; set; }
    }
}