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

            // Get users
            var adminUser = await userManager.FindByEmailAsync("admin@corporatetravel.com");
            var managerUser = await userManager.FindByEmailAsync("manager@corporatetravel.com");
            var regularUser = await userManager.FindByEmailAsync("user@corporatetravel.com");

            var sampleRequests = new List<TravelRequest>();

            // Admin user requests
            if (adminUser != null)
            {
                sampleRequests.AddRange(new[]
                {
                    new TravelRequest
                    {
                        RequestingUserId = adminUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "New York, USA",
                        Reason = "Client Meeting",
                        StartDate = DateTime.Now.AddDays(30),
                        EndDate = DateTime.Now.AddDays(33),
                        Status = TravelRequestStatus.Approved,
                        CreatedAt = DateTime.Now.AddDays(-10),
                        UpdatedAt = DateTime.Now.AddDays(-8)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = adminUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "Paris, France",
                        Reason = "Training Session",
                        StartDate = DateTime.Now.AddDays(20),
                        EndDate = DateTime.Now.AddDays(25),
                        Status = TravelRequestStatus.Approved,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        UpdatedAt = DateTime.Now.AddDays(-1)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = adminUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "Berlin, Germany",
                        Reason = "Trade Show",
                        StartDate = DateTime.Now.AddDays(40),
                        EndDate = DateTime.Now.AddDays(44),
                        Status = TravelRequestStatus.Approved,
                        CreatedAt = DateTime.Now.AddDays(-8),
                        UpdatedAt = DateTime.Now.AddDays(-6)
                    }
                });
            }

            // Manager user requests
            if (managerUser != null)
            {
                sampleRequests.AddRange(new[]
                {
                    new TravelRequest
                    {
                        RequestingUserId = managerUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "London, UK",
                        Reason = "Conference Attendance",
                        StartDate = DateTime.Now.AddDays(45),
                        EndDate = DateTime.Now.AddDays(50),
                        Status = TravelRequestStatus.Pending,
                        CreatedAt = DateTime.Now.AddDays(-5),
                        UpdatedAt = DateTime.Now.AddDays(-5)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = managerUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "Sydney, Australia",
                        Reason = "Partner Meeting",
                        StartDate = DateTime.Now.AddDays(75),
                        EndDate = DateTime.Now.AddDays(80),
                        Status = TravelRequestStatus.Pending,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        UpdatedAt = DateTime.Now.AddDays(-2)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = managerUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "Singapore",
                        Reason = "Regional Office Visit",
                        StartDate = DateTime.Now.AddDays(90),
                        EndDate = DateTime.Now.AddDays(95),
                        Status = TravelRequestStatus.Pending,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        UpdatedAt = DateTime.Now.AddDays(-1)
                    }
                });
            }

            // Regular user requests
            if (regularUser != null)
            {
                sampleRequests.AddRange(new[]
                {
                    new TravelRequest
                    {
                        RequestingUserId = regularUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "Tokyo, Japan",
                        Reason = "Business Development",
                        StartDate = DateTime.Now.AddDays(60),
                        EndDate = DateTime.Now.AddDays(67),
                        Status = TravelRequestStatus.Rejected,
                        CreatedAt = DateTime.Now.AddDays(-15),
                        UpdatedAt = DateTime.Now.AddDays(-12)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = regularUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "Toronto, Canada",
                        Reason = "Team Building",
                        StartDate = DateTime.Now.AddDays(55),
                        EndDate = DateTime.Now.AddDays(60),
                        Status = TravelRequestStatus.Rejected,
                        CreatedAt = DateTime.Now.AddDays(-7),
                        UpdatedAt = DateTime.Now.AddDays(-5)
                    },
                    new TravelRequest
                    {
                        RequestingUserId = regularUser.Id,
                        Origin = "São Paulo, Brazil",
                        Destination = "Miami, USA",
                        Reason = "Vacation",
                        StartDate = DateTime.Now.AddDays(100),
                        EndDate = DateTime.Now.AddDays(105),
                        Status = TravelRequestStatus.Pending,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        UpdatedAt = DateTime.Now.AddDays(-1)
                    }
                });
            }

            await context.TravelRequests.AddRangeAsync(sampleRequests);
            await context.SaveChangesAsync();
        }
    }
} 