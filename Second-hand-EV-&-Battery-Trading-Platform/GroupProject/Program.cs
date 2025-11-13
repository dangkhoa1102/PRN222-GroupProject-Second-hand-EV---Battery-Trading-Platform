using BLL.Configuration;
using BLL.Services;
using DAL.Repositories.Admin;

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

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVehicleListingService, VehicleListingService>();
builder.Services.AddScoped<IBatteryListingService, BatteryListingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddAdminTransactionService(builder.Configuration);
builder.Services.AddScoped<IAdminReviewService, AdminReviewService>();
//builder.Services.AddScoped<IAdminReviewRepository, AdminReviewRepository>();


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
