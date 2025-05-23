﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container
builder.Services.AddControllersWithViews();

// Đọc cấu hình MongoDB từ appsettings.json
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

// Đăng ký các dịch vụ MongoDbService và UserService
builder.Services.AddSingleton<MongoDbService>();  // Singleton cho MongoDbService
builder.Services.AddScoped<IUserService, UserService>();  // Scoped cho UserService
builder.Services.AddScoped<IPostService, PostService>(); // Cũng phải đăng ký MongoDB Database instance và kết nối cho PostService nhận

// Đăng ký session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Tạo ứng dụng
var app = builder.Build();

// Cấu hình pipeline xử lý HTTP request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");  // Xử lý lỗi trong môi trường sản xuất
    app.UseHsts();  // Đảm bảo HTTP Strict Transport Security
}

// Middleware cho bảo mật và yêu cầu HTTPS
app.UseHttpsRedirection();
app.UseRouting();

// Đảm bảo yêu cầu xác thực
app.UseAuthorization();
// Sử dụng session
app.UseSession();
// Xử lý tài nguyên tĩnh (assets)
app.UseStaticFiles();
// Định nghĩa route mặc định cho MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");  // Cấu hình route mặc định cho ứng dụng

// Chạy ứng dụng
app.Run();
