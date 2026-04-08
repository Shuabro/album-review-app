using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using backend.Data;
using backend.Models;

namespace UserSeeder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            // WARNING: Sensitive connection string removed. Use environment variables or user-secrets in production.
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("ERROR: CONNECTION_STRING environment variable is not set.");
                return;
            }
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create first user
            await CreateUser(userManager, "jminor", "jcminor331@gmail.com", "Password123!", "John", "Minor");
            
            // Create second user (uncomment if needed)
            await CreateUser(userManager, "bishibro", "jpowerscts@gmail.com", "Password123!", "Joshua", "Powers");

            Console.WriteLine("User seeding complete. Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task CreateUser(UserManager<ApplicationUser> userManager, string userName, string email, string password, string firstName, string lastName)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApplicationUser 
                { 
                    UserName = userName, 
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    Console.WriteLine($"User '{userName}' created successfully. Id: {user.Id}");
                    Console.WriteLine($"Full Name: {user.FullName}");
                }
                else
                {
                    Console.WriteLine($"Failed to create user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"User '{userName}' already exists. Id: {user.Id}");
                Console.WriteLine($"Full Name: {user.FullName}");
            }
        }
    }
}