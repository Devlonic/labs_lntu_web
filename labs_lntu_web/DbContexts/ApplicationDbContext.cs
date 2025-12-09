using labs_lntu_web.Models;
using Microsoft.EntityFrameworkCore;

namespace labs_lntu_web.DbContexts {
    public class ApplicationDbContext : DbContext {
        public DbSet<Item> Items => Set<Item>();
        public DbSet<HostData> Hosts => Set<HostData>();
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
            Database.EnsureCreated();
        }
    }
}
