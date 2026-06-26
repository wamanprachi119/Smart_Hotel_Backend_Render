using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Railway uses PORT automatically
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Read connection string from Railway Variables first,
// otherwise use local appsettings.json
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=smart_hotel;User=root;Password=root;AllowPublicKeyRetrieval=true;SslMode=None;";

builder.Services.AddDbContext<SmartHotelContext>(options =>
    options.UseMySql(
        connStr,
        ServerVersion.AutoDetect(connStr)
    ));

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SmartHotelContext>();

        if (await db.Database.CanConnectAsync())
        {
            Console.WriteLine("✅ Database Connected");
        }
        else
        {
            Console.WriteLine("❌ Database Disconnected");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.MapControllers();

app.MapGet("/", () => "Smart Hotel Backend Running");

app.MapGet("/health", async (SmartHotelContext db) =>
{
    try
    {
        var connected = await db.Database.CanConnectAsync();

        return Results.Ok(new
        {
            status = "ok",
            database = connected ? "connected" : "disconnected"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "error",
            message = ex.Message
        });
    }
});

app.Run();