using Microsoft.EntityFrameworkCore;
using PantryTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure DbContext with SQLite
builder.Services.AddDbContext<PantryDbContext>(options =>
    options.UseSqlite("Data Source=pantry.db"));

// Add CORS to allow frontend to call API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PantryDbContext>();
    context.Database.EnsureCreated();
    PantryDbContext.Seed(context);
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
