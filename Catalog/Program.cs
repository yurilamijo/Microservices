using Catalog.Models;
using GenericRepository.Identity;
using GenericRepository.MassTransit;
using GenericRepository.MongoDb;
using GenericRepository.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var MyAllowSpecificOriginsSetting = "AllowedOrigin";

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policyBuilder =>
                      {
                          policyBuilder.WithOrigins(builder.Configuration[MyAllowSpecificOriginsSetting])
                                  .AllowAnyHeader()
                                  .AllowAnyMethod();
                      });
});

// Add services to the container.
builder.Services.AddMongo()
        .AddMongoRepository<Item>("items")
        .AddMassTransitWithRabbitMq()
        .AddJwtBearerAuthentication();

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(MyAllowSpecificOrigins);
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
