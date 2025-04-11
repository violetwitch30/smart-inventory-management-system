using SmartInventoryManagementSystem.Areas.ProjectManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace SmartInventoryManagementSystem.Data;

public class ContextSeed
{
    public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager )
    {
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.Admin.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.User.ToString()));
    }

    public static async Task SuperSeedRolesAsync(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@domain.ca",
            FirstName = "Admin",
            LastName = "Account",
            ContactInformation = "admin-support",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
        };
        
        if (userManager.Users.All(u => u.Id != adminUser.Id))
        {
            var user = await userManager.FindByEmailAsync(adminUser.Email);
            
            if (user == null)
            {
                // Create the superuser account with the specified password
                await userManager.CreateAsync(adminUser, "AdminPass123!");
                
                // Assign the superuser with all the following roles
                await userManager.AddToRoleAsync(adminUser, Enum.Roles.Admin.ToString());
            }
        }
    }
}