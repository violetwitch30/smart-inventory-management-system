using SmartInventoryManagementSystem.Areas.ProjectManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace SmartInventoryManagementSystem.Data;

public class ContextSeed
{
    public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager )
    {
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.SuperAdmin.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.Admin.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.Moderator.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.Basic.ToString()));
    }

    public static async Task SuperSeedRolesAsync(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var superUser = new ApplicationUser
        {
            UserName = "superadmin",
            Email = "adminsupport@domain.ca",
            FirstName = "Super",
            LastName = "Admin",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
        };

        // Check if the super user does not already exists in the datbase
        if (userManager.Users.All(u => u.Id != superUser.Id))
        {
            var user = await userManager.FindByEmailAsync(superUser.Email);

            // If the superuser account does not exist, proceed with creating it
            if (user == null)
            {
                // Create the superuser account with the specified password
                await userManager.CreateAsync(superUser, "SuperPass123!");
                
                // Assign the superuser with all the following roles
                await userManager.AddToRoleAsync(superUser, Enum.Roles.SuperAdmin.ToString());
                await userManager.AddToRoleAsync(superUser, Enum.Roles.Admin.ToString());
                await userManager.AddToRoleAsync(superUser, Enum.Roles.Moderator.ToString());
                await userManager.AddToRoleAsync(superUser, Enum.Roles.Basic.ToString());
            }
        }
    }
}