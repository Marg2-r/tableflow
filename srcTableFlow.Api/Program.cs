using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TableFlow.Api.Data;
using TableFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<TableFlowDbContext>(options =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection");

    options.UseNpgsql(connectionString);
});

builder.Services
    .AddScoped<ReservationAvailabilityService>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("Frontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<TableFlowDbContext>();

    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Tableflow API v1");
    });

}
//Leave this disabled for now because you are running HTTP locally
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

