using ManagementCenter.Models;
using ManagementCenter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection; // Quan trọng cho AddLocalization
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(); // Đảm bảo không lỗi ở dòng này (cần using)

// Cấu hình RequestLocalizationOptions
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("vi-VN") // BẮT BUỘC CÓ VI-VN
    };

    // Đặt culture mặc định là vi-VN
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    // Khai báo các culture được hỗ trợ
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>()?.Value;
if (localizationOptions != null)
{
    app.UseRequestLocalization(localizationOptions); // Áp dụng cấu hình culture
}
else
{
    // Log hoặc fallback nếu cần
    app.Logger.LogWarning("RequestLocalizationOptions not found. Applying minimal fallback settings.");
    var fallbackOptions = new RequestLocalizationOptions()
       .SetDefaultCulture("vi-VN")
       .AddSupportedCultures("vi-VN")
       .AddSupportedUICultures("vi-VN");
    app.UseRequestLocalization(fallbackOptions);
}



app.UseStaticFiles(); // Sử dụng Static Files

app.UseAuthentication(); // Xác thực người dùng
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// ----- (Tùy chọn nhưng nên có) Seed Roles và Admin User -----
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>(); // Lấy logger

        // Lời gọi hàm InitializeAsync từ class RoleInitializer trong namespace Data
        await RoleInitializer.InitializeAsync(userManager, roleManager); // <<< Giữ nguyên lời gọi này
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }


}
// ----- Kết thúc phần Seed Data -----

app.Run();
