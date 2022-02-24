using GenericRepository.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GenericRepository.Repositories
{
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> dbCollection;

        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            dbCollection = database.GetCollection<T>(collectionName);
        }

        /*
         * Gets all entities from the database
         */
        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }

        /*
         * Gets filterd all entities from the database
         */
        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).ToListAsync();
        }

        /*
         * Gets a single entity from the database
         */
        public async Task<T> GetAsync(Guid id)
        {
            FilterDefinition<T> filteredEntity = filterBuilder.Eq(entity => entity.Id, id);
            return await dbCollection.Find(filteredEntity).FirstOrDefaultAsync();
        }

        /*
         * Gets a filterd single entity from the database
         */
        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        /*
         * Creates a new entity in the database
         */
        public async Task CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await dbCollection.InsertOneAsync(entity);
        }

        /*
         * Updates a new entity in the database
         */
        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            FilterDefinition<T> filteredEntity = filterBuilder.Eq(exisitingEntity => exisitingEntity.Id, entity.Id);

            await dbCollection.ReplaceOneAsync(filteredEntity, entity);
        }

        /*
         * Delete a entity in the database
         */
        public async Task DeleteAsync(Guid id)
        {
            FilterDefinition<T> filteredEntity = filterBuilder.Eq(entity => entity.Id, id);

            if (filteredEntity == null)
            {
                throw new ArgumentNullException(nameof(filteredEntity));
            }

            await dbCollection.DeleteOneAsync(filteredEntity);
        }
    }
}
