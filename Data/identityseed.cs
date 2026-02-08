using Microsoft.AspNetCore.Identity;
namespace BeReadtForExam.Data
{
    public static class identityseed
    {

        public static async Task SeedAsync(IServiceProvider services) 
        { 
            using var scope = services.CreateScope(); 
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(); 
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string[] roles = { "Admin", "Teacher", "Student" }; 
            foreach (var role in roles)
            { 
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role)); 
            } 
            await EnsureUserWithRole(userManager, "admin@school.local", "Admin123!", "Admin");
            await EnsureUserWithRole(userManager, "teacher@school.local", "Teacher123!", "Teacher"); 
            await EnsureUserWithRole(userManager, "student@school.local", "Student123!", "Student"); 
        }
        private static async Task EnsureUserWithRole(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) 
            {
                user = new IdentityUser 
                { UserName = email, Email = email, EmailConfirmed = true  };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description)); 
                    throw new Exception($"Cannot create user {email}: {errors}"); 
                } 
            }
            if (!await userManager.IsInRoleAsync(user, role)) 
                await userManager.AddToRoleAsync(user, role);
        }
    }

}
