using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Apex.Catering.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build SQLite connection string that matches the DbContext's local app-data file
var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var dbPath = Path.Join(localAppData, "Apex.catering.db");
var connectionString = $"Data Source={dbPath}";

// Register the CateringDbContext with an explicit SQLite connection string
builder.Services.AddDbContext<CateringDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    AddData(app);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Diagnostic: list mapped endpoints so you can confirm exact routes
app.MapGet("/__endpoints", (EndpointDataSource ds) =>
{
    var routes = ds.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => new { Pattern = e.RoutePattern.RawText, Name = e.DisplayName })
        .OrderBy(r => r.Pattern);
    return Results.Json(routes);
});

app.Run();

void AddData(IHost app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<CateringDbContext>();
    var dbInitializer = new DbDataInitializer(context);
    dbInitializer.InitializeData();
}