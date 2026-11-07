
using FoodieHub.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();
builder.Services.AddControllers();
// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FoodieHubPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Database connection
builder.Services.AddDbContext<FoodieHubDbContext>(options =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection");

    options.UseMySql(
        connectionString,
        new MariaDbServerVersion(new Version(10, 4, 32))
    );
});

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("FoodieHubPolicy");

app.MapControllers();

app.Run();
