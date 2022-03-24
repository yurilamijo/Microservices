using GenericRepository.Settings;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GenericRepository.MassTransit
{
    public static class Extensions
    {
        /// <summary>
        /// Adds MassTransit with RabbitMQ
        /// </summary>
        /// <param name="services">IServiceCollection that gets extended</param>
        /// <returns>A IServiceCollection with MassTransit and RabbitMQ</returns>
        public static IServiceCollection AddMassTransitWithRabbitMq(
            this IServiceCollection services,
            Action<IRetryConfigurator>? configureRetries = null)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingCustomRabbitMq(configureRetries);
            });

            services.AddMassTransitHostedService();

            return services;
        }

        public static void UsingCustomRabbitMq(
            this IServiceCollectionBusConfigurator configure,
            Action<IRetryConfigurator>? configureRetries)
        {
            configure.UsingRabbitMq((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configurator.Host(rabbitMQSettings.Host);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));

                if (configureRetries == null)
                {
                    configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }

                // Retry sending to the consumer
                configurator.UseMessageRetry(configureRetries);
            });
        }
    }
}
