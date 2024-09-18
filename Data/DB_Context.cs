using I72_Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace I72_Backend.Data
{
    public class DB_Context : DbContext
    {
        public DB_Context(DbContextOptions<DB_Context> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Dynamic> Dynamic { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

          
        }
    }
}
