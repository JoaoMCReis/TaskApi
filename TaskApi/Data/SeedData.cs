using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskApi.Entities;

namespace TaskApi.Data
{
    public static class SeedData
    {
        public static void EnsureSeedData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();

            if (db.Users.Any()) return;

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "admin",
                Name = "Admnistrator"
            };
            
            var member = new User
            {
                Id = Guid.NewGuid(),
                Email = "member@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member123!"),
                Role = "member",
                Name = "Menbro"
            };

            db.Users.AddRange(admin, member);
            db.SaveChanges();
        }
    }
}