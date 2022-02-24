using GenericRepository.Models;
using GenericRepository.Repositories;
using GenericRepository.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace GenericRepository.MongoDb
{
    public static class Extensions
    {
        /// <summary>
        /// Service collection extension methode for MongoDB
        /// </summary>
        /// <param name="services">IServiceCollection that gets extended</param>
        /// <returns>A IServiceCollection with a single MongoDb client</returns>
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            // Added serializers for readablity
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            // MongoDB configuration
            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            return services;
        }

        /// <summary>
        /// Adds a generic MongoDb repository
        /// </summary>
        /// <typeparam name="T">Generic type of IEntity</typeparam>
        /// <param name="services">IServiceCollection that gets extended</param>
        /// <param name="collectionName">Name of the collection</param>
        /// <returns>A IServiceCollection with a repository</returns>
        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName)
            where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<T>(database, collectionName);
            });

            return services;
        }
    }
}
