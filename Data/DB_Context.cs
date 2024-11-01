using I72_Backend.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

        public async Task<(bool success, string message)> EnsureDataExistsAsync()
        {
            _logger.LogInformation("Starting EnsureDataExistsAsync method...");

            try
            {
                // Check database connection
                if (!Database.CanConnect())
                {
                    _logger.LogInformation("Database doesn't exist, creating...");
                    await Database.EnsureCreatedAsync();
                }

                // Check if tables exist
                var databaseCreator = Database.GetService<IRelationalDatabaseCreator>();
                if (!databaseCreator.HasTables())
                {
                    _logger.LogInformation("Tables don't exist, creating...");
                    databaseCreator.CreateTables();
                    return await SeedDataAsync("Tables created and data seeded successfully.");
                }

                // Check if Users table is empty
                if (!await Users.AnyAsync())
                {
                    _logger.LogInformation("Users table is empty, seeding data...");
                    return await SeedDataAsync("Data seeded successfully into empty table.");
                }

                _logger.LogInformation("Tables and data already exist, no action needed.");
                return (true, "Database is already populated.");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage);
            }
        }

        private async Task<(bool success, string message)> SeedDataAsync(string successMessage)
        {
            try
            {
                var seedUsers = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        FirstName = "System",
                        LastName = "Admin",
                        Phone = "0501234567",
                        Role = "Admin"
                    },
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
                        Username = "Ali11",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        FirstName = "Ali",
                        LastName = "Khan",
                        Phone = "0501315487",
                        Role = "Staff"
                    },
                    new User
                    {
                        Username = "staff",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        FirstName = "System",
                        LastName = "Staff",
                        Phone = "0507654321",
                        Role = "Staff"
                    }
                };

                await Users.AddRangeAsync(seedUsers);
                await SaveChangesAsync();

                _logger.LogInformation($"Successfully seeded {seedUsers.Count} users.");
                return (true, successMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to seed data: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage);
            }
        }

        
        public async Task<(bool success, string message)> ReseedDataAsync()
        {
            try
            {
                // Remove existing data
                if (await Users.AnyAsync())
                {
                    Users.RemoveRange(Users);
                    await SaveChangesAsync();
                    _logger.LogInformation("Existing data removed successfully.");
                }

                // Reseed the data
                return await SeedDataAsync("Data reseeded successfully.");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to reseed data: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage);
            }
        }
    }
}