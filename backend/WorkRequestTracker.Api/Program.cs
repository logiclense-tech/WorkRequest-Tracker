using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WorkRequestDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WorkRequests")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.MapControllers();

// Convenient for the assignment demo. Prefer EF Core migrations in a maintained application.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorkRequestDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
