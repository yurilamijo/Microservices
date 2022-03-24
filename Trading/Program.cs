using GenericRepository.Identity;
using GenericRepository.MassTransit;
using GenericRepository.MongoDb;
using GenericRepository.Settings;
using MassTransit;
using Trading.StateMachines;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo()
    .AddJwtBearerAuthentication();

// MassTransit configuration
builder.Services.AddMassTransit(configure =>
{
    configure.UsingCustomRabbitMq(null);
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

builder.Services.AddControllers();
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