using BLL.Services;
using BLL.Configuration;
using GroupProject.Services.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
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

// Register Background Services from HEAD branch
builder.Services.AddHostedService<OrderAutoCancelService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
