using COMP2139_Assignment1.Data;
using COMP2139_Assignment1.Models;
using COMP2139_Assignment1.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace COMP2139_Assignment1;

public class Program
{
    public static async Task Main(string[] args) 
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        
        builder.Services.AddTransient<IEmailSender, EmailSender>();
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>() 
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var app = builder.Build();

        // Seed roles (and optionally default admin)
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await SeedRolesAsync(roleManager);
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseStatusCodePagesWithReExecute("/Error/{0}");

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        app.MapRazorPages();

        await app.RunAsync(); 
    }

    // Role seeding method
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "User" };

        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role: {role}");
                }
            }
        }
    }
}
