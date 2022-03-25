using GenericRepository.Identity;
using GenericRepository.MassTransit;
using GenericRepository.MongoDb;
using GenericRepository.Settings;
using GreenPipes;
using MassTransit;
using System.Reflection;
using Trading.Entities;
using Trading.Exceptions;
using Trading.StateMachines;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo()
    .AddMongoRepository<CatalogItem>("catalogitems")
    .AddJwtBearerAuthentication();

// MassTransit configuration
builder.Services.AddMassTransit(configure =>
{
    configure.UsingCustomRabbitMq(retryConfigurator =>
    {
        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
        retryConfigurator.Ignore(typeof(UnknownItemException));
    });
    // Add consumers that are in this assembly so that MassTranit can use them
    configure.AddConsumers(Assembly.GetEntryAssembly());
    configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>()
        .MongoDbRepository(r =>
        {
            var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            r.Connection = mongoSettings.ConnectionString;
            r.DatabaseName = serviceSettings.ServiceName;
        });
});

builder.Services.AddMassTransitHostedService();

builder.Services.AddGenericRequestClient();

builder.Services.AddControllers(options =>
{
    // Suppress removal of 'async' in method names 
    options.SuppressAsyncSuffixInActionNames = false;
}).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.IgnoreNullValues = true;
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
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();