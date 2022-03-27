using GenericRepository.Identity;
using GenericRepository.MassTransit;
using GenericRepository.MongoDb;
using GenericRepository.Settings;
using GreenPipes;
using MassTransit;
using System.Reflection;
using Trading.Entities;
using Trading.Exceptions;
using Trading.Settings;
using Trading.StateMachines;
using static Contracts.IdentityContracts;
using static Contracts.InventoryContracts;

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
    configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
    {
        // Delays the message until the state has changed
        sagaConfigurator.UseInMemoryOutbox();
    })
        .MongoDbRepository(r =>
        {
            var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            r.Connection = mongoSettings.ConnectionString;
            r.DatabaseName = serviceSettings.ServiceName;
        });
});

var queueSettings = builder.Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();

// Endpoint conventions
EndpointConvention.Map<GrandItems>(new Uri(queueSettings.GrandItemsQueueAddress));
EndpointConvention.Map<DebitPoints>(new Uri(queueSettings.DebitPointsQueueAddress));

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