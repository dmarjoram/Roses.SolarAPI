using Microsoft.AspNetCore.Mvc;
using Roses.SolarAPI.Serialization;
using Roses.SolarAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ForecastService>();
builder.Services.AddSingleton<FoxESSService>();

builder.Services.AddControllers();

// Support serialisation of TimeOnly in .NET 6
builder.Services.Configure<JsonOptions>(options => options.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
