// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace PhoenixLib.DAL.MongoDB
{
    public sealed class SynchronizedMongoRepository<TObject> : IGenericAsyncUuidRepository<TObject> where TObject : class, IUuidDto
    {
        private readonly IMongoCollection<TObject> Collection;

        // protected readonly ILogger Log;
        private readonly IMongoDatabase Database;

        public SynchronizedMongoRepository(MongoConfiguration conf)
        {
            var settings = MongoClientSettings.FromConnectionString(conf.ToString());
            var client = new MongoClient(settings);
            Database = client.GetDatabase(conf.DatabaseName);
            Collection = Database.GetCollection<TObject>(typeof(TObject).Name);
        }

        public async Task<IEnumerable<TObject>> GetAllAsync() => await (await Collection.FindAsync(FilterDefinition<TObject>.Empty)).ToListAsync();

        public async Task<TObject> GetByIdAsync(Guid id)
        {
            return await (await Collection.FindAsync(o => o.Id == id)).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TObject>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await (await Collection.FindAsync(o => ids.Any(s => s == o.Id))).ToListAsync();
        }

        public async Task<TObject> SaveAsync(TObject obj)
        {
            return await Collection.FindOneAndUpdateAsync(o => o.Id == obj.Id, new ObjectUpdateDefinition<TObject>(obj));
        }

        public async Task<IEnumerable<TObject>> SaveAsync(IReadOnlyList<TObject> objs)
        {
            // probably faster than FindOneAndUpdateOne for each object
            await Collection.DeleteManyAsync(o => objs.Any(s => s.Id == o.Id));
            await Collection.InsertManyAsync(objs);
            return objs.ToList();
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            await Collection.DeleteOneAsync(obj => obj.Id == id);
        }

        public async Task DeleteByIdsAsync(IEnumerable<Guid> ids)
        {
            await Collection.DeleteManyAsync(o => ids.Any(id => o.Id == id));
        }
    }
}