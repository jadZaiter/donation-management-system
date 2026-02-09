using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Infrastructure.Repositories;
using DonationManagementSystem.Infrastructure.Services;
using DonationManagementSystem.Web.BackgroundServices;
using DonationManagementSystem.Web.Hubs;
using DonationManagementSystem.Web.MappingProfiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
}).AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// AutoMapper Registration
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// MVC
builder.Services.AddControllersWithViews();

// ? Register Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDonationCaseService, DonationCaseService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<PaymentService>(); // ? ADD THIS
builder.Services.AddScoped<DonationCaseWorkflow>();
builder.Services.AddScoped<PaymentWorkflow>(); // ? ADD THIS

// Background Services
builder.Services.AddHostedService<DonationCaseAutoCloseService>();
builder.Services.AddHostedService<DonationManagementSystem.Web.BackgroundServices.DonationCaseMonitorService>();

// SignalR
builder.Services.AddSignalR();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

Log.Information("Application started successfully");

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Create Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    // Promote a specific user to Admin
    var adminEmail = "jad_wb@hotmail.com";
    var user = await userManager.FindByEmailAsync(adminEmail);

    if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
        await userManager.AddToRoleAsync(user, "Admin");
}

// ? Make sure this is in the middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ? API endpoints mapping
app.MapControllers(); // ? This line is CRITICAL for API controllers

// MVC endpoints mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// This should work with routes like:
// /Admin/Approve/40 -> AdminController.Approve(40)
// /Admin/Reject/40 -> AdminController.Reject(40)

app.MapRazorPages();

app.Run();





























































































































































































































