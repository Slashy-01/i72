using I72_Backend.Data;
using I72_Backend.Interfaces;
using I72_Backend.Repository;
using I72_Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace I72_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                        ClockSkew = TimeSpan.Zero // Eliminate clock skew to prevent token expiry issues
                    };
                });

            // Authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
            });

            // Register services and repositories
            builder.Services.AddControllers();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            // Configure the database context with MySQL and automatic server version detection
            builder.Services.AddDbContext<DB_Context>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });


            // API Explorer and Swagger Configuration
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                // Add JWT Bearer token support in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token in the format: Bearer {token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<DB_Context>();
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogInformation("Ensuring database is created...");
                    context.Database.EnsureCreated();

                    logger.LogInformation("Applying any pending migrations...");
                    context.Database.Migrate();

                    logger.LogInformation("Seeding data...");
                    context.EnsureSeedData();

                    logger.LogInformation("Database setup completed successfully.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while setting up the database.");
                }
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Ensure that Authentication is before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
