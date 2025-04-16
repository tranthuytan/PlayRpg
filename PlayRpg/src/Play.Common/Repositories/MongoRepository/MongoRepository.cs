using System.Linq.Expressions;
using MongoDB.Driver;
using Play.Common.Entities;

namespace Play.Common.Repositories
{
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        // an actual object represent MongoCollection
        private readonly IMongoCollection<T> dbCollection;
        // filter builder to query for data in mongo
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            // var mongoClient = new MongoClient("mongodb://localhost:27017"); //without IMongoDatabase dependency injection
            // var database = mongoClient.GetDatabase("Catalog");
            dbCollection = database.GetCollection<T>(collectionName);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }
        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).ToListAsync();
        }
        public async Task<T> GetByIdAsync(Guid id)
        {
            return await dbCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
        }
        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            await dbCollection.InsertOneAsync(entity);
        }
        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            FilterDefinition<T> filter = filterBuilder.Eq(i => i.Id, entity.Id);
            await dbCollection.ReplaceOneAsync(filter, entity);
        }
        public async Task DeleteAsync(Guid id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(i => i.Id, id);
            await dbCollection.DeleteOneAsync(filter);
        }
    }
}