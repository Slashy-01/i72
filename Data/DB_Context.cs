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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    username = "John",
                    password = "Doe",
                },
                new User
                {
                    Id = 2,
                    username = "Jane",
                    password = "Doe",
                }
            );
        }
    }
}
