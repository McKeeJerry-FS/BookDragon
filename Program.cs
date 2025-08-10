using BookDragon.Data;
using BookDragon.Models;
using BookDragon.Services;
using BookDragon.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration);
if (string.IsNullOrEmpty(connectionString))
{
    // Log environment variables for debugging
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var pgHost = Environment.GetEnvironmentVariable("PGHOST");
    var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
    
    throw new InvalidOperationException($"Database connection string is required. DATABASE_URL: {databaseUrl}, PGHOST: {pgHost}, PGDATABASE: {pgDatabase}");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IImageService, ImageService>();

// Add Email Service
builder.Services.AddTransient<IEmailSender, EmailService>();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Apply database migrations in production
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DataUtility.ManageDataAsync(scope.ServiceProvider);
}

app.Run();