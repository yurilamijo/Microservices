using Inventory.Clients;
using Inventory.Models;
using Polly;
using Polly.Timeout;
using static Inventory.Dtos;

namespace Inventory
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem item, string name, string description)
        {
            return new InventoryItemDto(item.CatalogItemId, name, description, item.Quantity, item.AcquiredDate);
        }

        public static IServiceCollection AddCatalogClient(this IServiceCollection services)
        {
            var jitterer = new Random();

            // Exponential backoff + Circuit breaker pattern
            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5263");
            })
            .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000))
            ))
            .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15)
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

            return services;
        }
    }
}
