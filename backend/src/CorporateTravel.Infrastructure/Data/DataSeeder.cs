using CorporateTravel.Domain.Entities;
using CorporateTravel.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CorporateTravel.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roleNames = { "Admin", "Manager", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new ApplicationRole(roleName));
                }
            }

            // Create Admin user
            var adminUser = await userManager.FindByEmailAsync("admin@corporatetravel.com");
            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser
                {
                    UserName = "admin@corporatetravel.com",
                    Email = "admin@corporatetravel.com",
                    Name = "Admin"
                };

                var createUserResult = await userManager.CreateAsync(newAdminUser, "Admin@123");
                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
            }
            else
            {
                var userRoles = await userManager.GetRolesAsync(adminUser);
                if (!userRoles.Contains("Admin"))
                {
                    if (userRoles.Contains("Manager"))
                    {
                        await userManager.RemoveFromRoleAsync(adminUser, "Manager");
                    }
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create Manager user
            var managerUser = await userManager.FindByEmailAsync("manager@corporatetravel.com");
            if (managerUser == null)
            {
                var newManagerUser = new ApplicationUser
                {
                    UserName = "manager@corporatetravel.com",
                    Email = "manager@corporatetravel.com",
                    Name = "Manager"
                };

                var createUserResult = await userManager.CreateAsync(newManagerUser, "Manager@123");
                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newManagerUser, "Manager");
                }
            }
            else
            {
                var userRoles = await userManager.GetRolesAsync(managerUser);
                if (!userRoles.Contains("Manager"))
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }

            // Create User user
            var regularUser = await userManager.FindByEmailAsync("user@corporatetravel.com");
            if (regularUser == null)
            {
                var newRegularUser = new ApplicationUser
                {
                    UserName = "user@corporatetravel.com",
                    Email = "user@corporatetravel.com",
                    Name = "Regular User"
                };

                var createUserResult = await userManager.CreateAsync(newRegularUser, "User@123");
                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newRegularUser, "User");
                }
            }
            else
            {
                var userRoles = await userManager.GetRolesAsync(regularUser);
                if (!userRoles.Contains("User"))
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }

            // Seed sample travel requests for all users
            await SeedSampleTravelRequestsAsync(context, userManager);
        }

        private static async Task SeedSampleTravelRequestsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Clear existing travel requests to ensure a clean slate on each run
            context.TravelRequests.RemoveRange(context.TravelRequests);
            await context.SaveChangesAsync();

            // Get users with "User" role only
            var usersInUserRole = await userManager.GetUsersInRoleAsync("User");
            
            if (!usersInUserRole.Any())
            {
                Console.WriteLine("No users found with 'User' role. Skipping travel request seeding.");
                return;
            }

            var sampleRequests = new List<TravelRequest>();
            var requestCounter = 1;

            // Create sample travel requests for each user with "User" role
            foreach (var user in usersInUserRole)
            {
                // Generate unique request codes for this user's requests
                var today = DateTime.UtcNow;
                var datePrefix = today.ToString("yyyyMMdd");
                var baseCode = $"TR-{datePrefix}";

                sampleRequests.AddRange(new[]
                {
                    new TravelRequest
                    {
                        RequestingUserId = user.Id,
                        RequestCode = $"{baseCode}-{requestCounter++:D4}",
                        Origin = "São Paulo, Brazil",
                        Destination = "New York, USA",
                        Reason = "Client Meeting",
                        StartDate = DateTime.UtcNow.AddDays(30),
                        EndDate = DateTime.UtcNow.AddDays(33),
                        Status = TravelRequestStatus.Approved,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        UpdatedAt = DateTime.UtcNow.AddDays(-8)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = user.Id,
                        RequestCode = $"{baseCode}-{requestCounter++:D4}",
                        Origin = "São Paulo, Brazil",
                        Destination = "Paris, France",
                        Reason = "Training Session",
                        StartDate = DateTime.UtcNow.AddDays(20),
                        EndDate = DateTime.UtcNow.AddDays(25),
                        Status = TravelRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = user.Id,
                        RequestCode = $"{baseCode}-{requestCounter++:D4}",
                        Origin = "São Paulo, Brazil",
                        Destination = "Berlin, Germany",
                        Reason = "Trade Show",
                        StartDate = DateTime.UtcNow.AddDays(40),
                        EndDate = DateTime.UtcNow.AddDays(44),
                        Status = TravelRequestStatus.Rejected,
                        CreatedAt = DateTime.UtcNow.AddDays(-8),
                        UpdatedAt = DateTime.UtcNow.AddDays(-6)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = user.Id,
                        RequestCode = $"{baseCode}-{requestCounter++:D4}",
                        Origin = "São Paulo, Brazil",
                        Destination = "London, UK",
                        Reason = "Conference Attendance",
                        StartDate = DateTime.UtcNow.AddDays(45),
                        EndDate = DateTime.UtcNow.AddDays(50),
                        Status = TravelRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        UpdatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = user.Id,
                        RequestCode = $"{baseCode}-{requestCounter++:D4}",
                        Origin = "São Paulo, Brazil",
                        Destination = "Sydney, Australia",
                        Reason = "Partner Meeting",
                        StartDate = DateTime.UtcNow.AddDays(75),
                        EndDate = DateTime.UtcNow.AddDays(80),
                        Status = TravelRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                });
            }

            await context.TravelRequests.AddRangeAsync(sampleRequests);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"Created {sampleRequests.Count} travel requests for {usersInUserRole.Count} user(s) with 'User' role.");
        }
    }
} 