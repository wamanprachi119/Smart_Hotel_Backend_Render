using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Railway uses PORT automatically
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Read connection string
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connStr))
{
    throw new Exception("ConnectionStrings:DefaultConnection not found.");
}

// Print connection string (hide password)
Console.WriteLine("========== DATABASE ==========");
Console.WriteLine(connStr.Replace(
    connStr.Split("Password=")[1].Split(';')[0],
    "********"));
Console.WriteLine("==============================");

// Configure EF Core
builder.Services.AddDbContext<SmartHotelContext>(options =>
{
    options.UseMySql(
        connStr,
        new MySqlServerVersion(new Version(8, 0, 36))
    );
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var allowedOrigins =
    Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[]
    {
        "http://localhost:5173",
        "http://localhost:3000"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Database test
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SmartHotelContext>();

        await db.Database.CanConnectAsync();

        // Uncomment ONE of these depending on your project

        // await db.Database.EnsureCreatedAsync();

        // await db.Database.MigrateAsync();

        Console.WriteLine("✅ MySQL Connected Successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Database Error");
        Console.WriteLine(ex.ToString());
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        message = "Smart Hotel Backend Running",
        status = "ok"
    });
});

app.MapGet("/health", async (SmartHotelContext db) =>
{
    try
    {
        var connected = await db.Database.CanConnectAsync();

        return Results.Ok(new
        {
            status = "ok",
            database = connected
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.ToString(),
            statusCode: 500
        );
    }
});

app.Run();