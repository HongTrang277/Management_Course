using Microsoft.AspNetCore.Identity;
using ManagementCenter.Models; // Cần using này
using System; // Cần using này
using System.Threading.Tasks; // Cần using này

namespace ManagementCenter.Data
{
    public class RoleInitializer
    {
        private static readonly string[] RoleNames = { "Admin", "Student" };
        private static readonly string AdminEmail = "aadmin@example.com";
        private static readonly string AdminPassword = "Password123!";

        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Tạo Roles nếu chưa tồn tại
            foreach (var roleName in RoleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            // Tạo Admin mẫu
            var adminUser = await userManager.FindByEmailAsync(AdminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = AdminEmail, Email = AdminEmail, EmailConfirmed = true, full_name = "Administrator" };
                var result = await userManager.CreateAsync(adminUser, AdminPassword);
                if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else { if (!await userManager.IsInRoleAsync(adminUser, "Admin")) await userManager.AddToRoleAsync(adminUser, "Admin"); }
        }
    }
}
