using GHent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GHent.Database
{
    public class GhentContext : DbContext
    {
        public DbSet<Album> Albums { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=GHent.db");
            base.OnConfiguring(optionsBuilder);
        }
    }
}