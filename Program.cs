using Vintellitour_Framework.Data;
using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Services.Interfaces;
using Vintellitour_Framework.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// ===== CRITICAL: Configure Form Options for File Upload =====
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB limit
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.MultipartBoundaryLengthLimit = int.MaxValue;
});

// Thêm các dịch vụ vào container
builder.Services.AddControllersWithViews();

// ===== Configure Request Size Limits =====
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});

// Đọc cấu hình từ appsettings.json
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));

// Email Services
builder.Services.AddTransient<IEmailSender, EmailSender>();

// MongoDB Services
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();

// Repository Services
builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();

// Business Services
builder.Services.AddScoped<IProvinceService, ProvinceService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IProductService, AdminProductService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IMomoService, MomoService>();

// MongoDB Client Configuration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetSection("MongoDB")["ConnectionString"];
    return new MongoClient(connectionString);
});

// MongoDB Database Configuration
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var config = sp.GetRequiredService<IConfiguration>();
    var dbName = config.GetSection("MongoDB")["DatabaseName"];
    return client.GetDatabase(dbName);
});

// MongoDB Context
var connectionString = builder.Configuration["MongoDB:ConnectionString"];
var databaseName = builder.Configuration["MongoDB:DatabaseName"];
builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    return new MongoDbContext(connectionString, databaseName);
});

// HTTP Client
builder.Services.AddHttpClient();

// Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Important for development
});

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ===== CORRECT MIDDLEWARE ORDER =====
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session must come BEFORE Authorization
app.UseSession();
app.UseAuthorization();

// Map routes
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Map}/{action=Index}/{id?}");

app.Run();