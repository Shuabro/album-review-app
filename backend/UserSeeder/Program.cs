using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using backend.Data;

namespace backend.Import
{
    public class UserSeeder
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer("Server=localhost;Database=AlbumReviewDb;Trusted_Connection=True;TrustServerCertificate=True;"));
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var userName = "bishibro";
            var email = "jpowerscts@gmail.com";
            var password = "Password123!";

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new IdentityUser { UserName = userName, Email = email };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    Console.WriteLine($"User '{userName}' created successfully. Id: {user.Id}");
                }
                else
                {
                    Console.WriteLine($"Failed to create user: {string.Join(", ", result.Errors)}");
                }
            }
            else
            {
                Console.WriteLine($"User '{userName}' already exists. Id: {user.Id}");
            }
        }
    }
}
