using Microsoft.EntityFrameworkCore;
using FigureShop.Models;
using Microsoft.AspNetCore.Identity;
using FigureShop.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Bổ sung đoạn này để cấu hình DbContext trước khi dùng Identity
builder.Services.AddDbContext<FigureShopContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));
// Nếu dùng SQL Server, thay bằng: UseSqlServer(...)

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Mật khẩu nhẹ hơn
    options.Password.RequireDigit = false;                 // Không cần số
    options.Password.RequireUppercase = false;             // Không cần chữ hoa
    options.Password.RequireLowercase = false;             // Không cần chữ thường
    options.Password.RequireNonAlphanumeric = false;       // Không cần ký tự đặc biệt
    options.Password.RequiredLength = 4;                   // Chỉ cần 4 ký tự
    options.Password.RequiredUniqueChars = 0;              // Không yêu cầu ký tự khác nhau

    // Tài khoản
    options.User.RequireUniqueEmail = true;

    // Khóa tài khoản khi đăng nhập sai
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<FigureShopContext>()
.AddDefaultTokenProviders();

// Gửi email
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Thêm MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "order",
    pattern: "Order/{action=Index}/{id?}",
    defaults: new { controller = "Order", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
