using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Infrastructure.Repositories;
using DonationManagementSystem.Infrastructure.Services;
using DonationManagementSystem.Web.BackgroundServices;
using DonationManagementSystem.Web.Hubs;
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

// MVC
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<PaymentWorkflow>();


builder.Services.AddScoped<IDonationCaseService, DonationCaseService>();
builder.Services.AddScoped<DonationCaseWorkflow>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHostedService<DonationCaseAutoCloseService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHostedService<DonationManagementSystem.Web.BackgroundServices.DonationCaseMonitorService>();

builder.Services.AddSignalR();



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

    // 1) Create Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    // 2) Promote a specific user to Admin (change this email)
    var adminEmail = "jad_wb@hotmail.com";
    var user = await userManager.FindByEmailAsync(adminEmail);

    if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
        await userManager.AddToRoleAsync(user, "Admin");
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // 
app.UseAuthorization();

app.MapHub<AdminNotificationHub>("/hubs/admin");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
