using BLL.Configuration;
using BLL.Services;
using DAL.Repositories.Admin;
using GroupProject.Services.BackgroundServices;
using GroupProject.Hubs;
using GroupProject.Services;
using System.Text;

// Set UTF-8 encoding for console and response
Console.OutputEncoding = Encoding.UTF8;
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        // Ensure UTF-8 encoding for Razor pages
    });

// Configure encoding
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVehicleListingService, VehicleListingService>();
builder.Services.AddScoped<IBatteryListingService, BatteryListingService>();

// Services from HEAD branch (buyer and order services)
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IBuyerOrderService, BuyerOrderService>();

// Services from main branch (review and admin services)
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddAdminTransactionService(builder.Configuration);
builder.Services.AddScoped<IAdminReviewService, AdminReviewService>();
// builder.Services.AddScoped<IAdminReviewRepository, AdminReviewRepository>();

// Register SignalR
builder.Services.AddSignalR();

// Register Notification Service
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register Background Services
builder.Services.AddHostedService<OrderAutoCancelService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configure encoding for responses
app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Response.ContentType))
    {
        context.Response.ContentType = "text/html; charset=utf-8";
    }
    else if (!context.Response.ContentType.Contains("charset"))
    {
        context.Response.ContentType += "; charset=utf-8";
    }
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
