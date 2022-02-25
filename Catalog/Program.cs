using Catalog.Models;
using GenericRepository.MassTransit;
using GenericRepository.MongoDb;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var MyAllowSpecificOriginsSetting = "AllowedOrigin";

var builder = WebApplication.CreateBuilder(args);

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
        .AddMassTransitWithRabbitMq();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
