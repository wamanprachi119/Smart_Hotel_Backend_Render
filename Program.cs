using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Railway injects PORT automatically
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Connection string — reads from env var ConnectionStrings__DefaultConnection first,
// then appsettings.json, then hardcoded local fallback
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=smart_hotel;User=root;Password=root;";

builder.Services.AddDbContext<SmartHotelContext>(options =>
    options.UseMySql(connStr, ServerVersion.Parse("8.0.0-mysql"))
           .EnableDetailedErrors()
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Smart Hotel API", Version = "v1" });
});

// CORS
var allowedOriginsEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
var allowedOrigins = !string.IsNullOrWhiteSpace(allowedOriginsEnv)
    ? allowedOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : new[]
    {
        "http://localhost:5173",
        "http://localhost:3000",
        "https://smarthotel.vercel.app"
    };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto-create tables on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SmartHotelContext>();
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect)
        {
            await db.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ MySQL connected — tables ready.");
        }
        else
        {
            Console.WriteLine("⚠️  Cannot connect to MySQL.");
            Console.WriteLine($"   Connection string: {connStr}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  DB error: {ex.Message}");
        Console.WriteLine("   App will start — fix DB and restart.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Hotel API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    message = "Smart Hotel Backend Running",
    status = "ok"
}));

app.MapGet("/health", async (SmartHotelContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "ok", database = canConnect ? "connected" : "disconnected" });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { status = "degraded", database = "error", error = ex.Message });
    }
});

app.Run();