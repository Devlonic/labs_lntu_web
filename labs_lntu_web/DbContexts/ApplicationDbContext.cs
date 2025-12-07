using labs_lntu_web.Models;
using Microsoft.EntityFrameworkCore;

namespace labs_lntu_web.DbContexts {
    public class ApplicationDbContext : DbContext {
        public DbSet<Item> Items => Set<Item>();
        public ApplicationDbContext() => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite("Data Source=application.db");
        }
    }
}
