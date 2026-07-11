using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TableFlowDbContext>(options =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection");

    options.UseNpgsql(connectionString);
});

builder.Services.AddOpenApi();

var app = builder.Build();

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

