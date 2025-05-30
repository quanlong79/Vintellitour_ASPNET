using Vintellitour_Framework.Data;
using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Services.Interfaces;
using Vintellitour_Framework.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container
builder.Services.AddControllersWithViews();



// Đọc cấu hình MongoDB từ appsettings.json
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Đăng ký các dịch vụ MongoDbService và UserService
builder.Services.AddSingleton<MongoDbService>();  // Singleton cho MongoDbService
builder.Services.AddScoped<IUserService, UserService>();  // Scoped cho UserService
builder.Services.AddScoped<IPostService, PostService>(); // Cũng phải đăng ký MongoDB Database instance và kết nối cho PostService nhận

builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IProvinceService, ProvinceService>();
builder.Services.AddScoped<ILocationService, LocationService>();


builder.Services.AddScoped<IProductService, AdminProductService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddScoped<CartService>();

// Đăng ký MongoClient singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetSection("MongoDB")["ConnectionString"];
    return new MongoClient(connectionString);
});
builder.Services.AddHttpClient();
// Đăng ký MongoDatabase scoped
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var config = sp.GetRequiredService<IConfiguration>();
    var dbName = config.GetSection("MongoDB")["DatabaseName"];
    return client.GetDatabase(dbName);
});

var connectionString = builder.Configuration["MongoDB:ConnectionString"];
var databaseName = builder.Configuration["MongoDB:DatabaseName"];

builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    return new MongoDbContext(connectionString, databaseName);
});

// Tạo AdminUserService và DashboardService
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<DashboardService>();
// Đăng ký session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//momo
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

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
app.MapControllers(); // Thêm dòng này để hỗ trợ API Controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Map}/{action=Index}/{id?}");
// Chạy ứng dụng
app.Run();
