using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace App.Models
{
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // bo tien to AspNet trong ten table khi tao table bang migrations
            // foreach (var identityType in modelBuilder.Model.GetEntityTypes()) {
            //     var tableName = identityType.GetTableName();
            //     if (tableName.StartsWith("AspNet"))
            //         identityType.SetTableName(tableName.Substring(6));
            // }

        } 
    }
}