using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Apex.Catering.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container and configure JSON to ignore cycles
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build SQLite connection string that matches the DbContext's local app-data file
var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var dbPath = Path.Join(localAppData, "Apex.catering.db");
var connectionString = $"Data Source={dbPath}";

// Register the CateringDbContext with an explicit SQLite connection string
builder.Services.AddDbContext<CateringDbContext>(options =>
    options.UseSqlite(connectionString));

// Development-friendly CORS: allow any localhost origin (useful when Events UI and API use different ports)
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalhostDev", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            try
            {
                var uri = new Uri(origin);
                return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                       || uri.Host.Equals("127.0.0.1");
            }
            catch
            {
                return false;
            }
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Run DB initialization and log result
AddData(app);

// Developer exception page + Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("LocalhostDev");

app.UseAuthorization();

app.MapControllers();

// Diagnostic: list mapped endpoints so you can confirm exact routes
// Hidden from Swagger/OpenAPI by ExcludeFromDescription()
app.MapGet("/__endpoints", (EndpointDataSource ds) =>
{
    var routes = ds.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => new { Pattern = e.RoutePattern.RawText, Name = e.DisplayName })
        .OrderBy(r => r.Pattern);
    return Results.Json(routes);
}).ExcludeFromDescription();

// Diagnostic: DB status (row counts)
app.MapGet("/__db/status", (IServiceProvider sp) =>
{
    using var scope = sp.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<CateringDbContext>();
    try
    {
        return Results.Json(new
        {
            FoodItems = ctx.FoodItems.Count(),
            Menus = ctx.Menus.Count(),
            MenuFoodItems = ctx.MenuFoodItems.Count(),
            FoodBookings = ctx.FoodBookings.Count()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
}).ExcludeFromDescription();

// Diagnostic: small sample (projected to avoid cycles)
app.MapGet("/__db/sample", (IServiceProvider sp) =>
{
    using var scope = sp.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<CateringDbContext>();
    try
    {
        var items = ctx.FoodItems
                       .AsNoTracking()
                       .Select(fi => new { fi.FoodItemId, fi.Description, fi.UnitPrice })
                       .ToList();

        var menus = ctx.Menus
                      .AsNoTracking()
                      .Select(m => new { m.MenuId, m.MenuName })
                      .ToList();

        var menuFood = ctx.MenuFoodItems
                         .AsNoTracking()
                         .Select(mf => new { mf.MenuId, mf.FoodItemId })
                         .ToList();

        var bookings = ctx.FoodBookings
                          .AsNoTracking()
                          .Select(b => new { b.FoodBookingId, b.MenuId, b.NumberOfGuests })
                          .ToList();

        return Results.Json(new { items, menus, menuFood, bookings });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
}).ExcludeFromDescription();

app.Run();

void AddData(IHost appHost)
{
    using var scope = appHost.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetService<ILoggerFactory>()?.CreateLogger("DbInitializer");
    try
    {
        var context = services.GetRequiredService<CateringDbContext>();
        logger?.LogInformation("Initializing Catering DB at {DbPath}", dbPath);

        // Try to apply migrations; fall back to EnsureCreated
        try
        {
            context.Database.Migrate();
            logger?.LogInformation("Applied migrations successfully.");
        }
        catch (Exception exM)
        {
            logger?.LogWarning(exM, "Migrate failed, falling back to EnsureCreated.");
            context.Database.EnsureCreated();
            logger?.LogInformation("EnsureCreated executed.");
        }

        var initializer = new DbDataInitializer(context);
        initializer.InitializeData();
        logger?.LogInformation("DbDataInitializer completed.");
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "Database initialization failed: {Message}", ex.Message);
        throw;
    }
}