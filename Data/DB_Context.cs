using I72_Backend.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace I72_Backend.Data
{
    public class DB_Context : DbContext
    {
        private readonly ILogger<DB_Context> _logger;

        public DB_Context(DbContextOptions<DB_Context> options, ILogger<DB_Context> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.Phone).IsRequired();
                entity.Property(e => e.Role).IsRequired();
            });
        }

        public void EnsureSeedData()
        {
            _logger.LogInformation("Starting EnsureSeedData method...");

            try
            {
                if (!Database.CanConnect())
                {
                    _logger.LogError("Cannot connect to the database.");
                    return;
                }

                _logger.LogInformation("Successfully connected to the database.");

                var seedUsers = new List<User>
                {
                    new User
                    {
                        Username = "mad@max",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        FirstName = "Max",
                        LastName = "Pain",
                        Phone = "0501357159",
                        Role = "Admin"
                    },
                    new User
                    {
                        Username = "Ali@1",
                        Password = BCrypt.Net.BCrypt.HashPassword("456789"),
                        FirstName = "Ali",
                        LastName = "Khan",
                        Phone = "0501315487",
                        Role = "Staff"
                    }
                };

                foreach (var seedUser in seedUsers)
                {
                    if (!Users.Any(u => u.Username == seedUser.Username))
                    {
                        Users.Add(seedUser);
                        _logger.LogInformation($"Adding seed user: {seedUser.Username}");
                    }
                    else
                    {
                        _logger.LogInformation($"Seed user already exists: {seedUser.Username}");
                    }
                }

                var entriesWritten = SaveChanges();
                _logger.LogInformation($"Seed data operation completed. {entriesWritten} new entries written.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}