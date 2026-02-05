// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace PhoenixLib.DAL.MongoDB
{
    public sealed class MappedMongoRepository<TObject> : IGenericAsyncLongRepository<TObject> where TObject : class, ILongDto
    {
        private readonly IMongoCollection<TObject> _collection;

        public MappedMongoRepository(MongoConfigurationBuilder builder, IMongoClient client) : this(builder.Build(), client)
        {
        }

        private MappedMongoRepository(MongoConfiguration conf, IMongoClient client)
        {
            IMongoDatabase database = client.GetDatabase(conf.DatabaseName);
            _collection = database.GetCollection<TObject>(typeof(TObject).Name);
        }

        public async Task<IEnumerable<TObject>> GetAllAsync() => await (await _collection.FindAsync(FilterDefinition<TObject>.Empty)).ToListAsync();

        public async Task<TObject> GetByIdAsync(long id)
        {
            return await (await _collection.FindAsync(o => o.Id == id)).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TObject>> GetByIdsAsync(IEnumerable<long> ids)
        {
            return await (await _collection.FindAsync(o => ids.Any(s => s == o.Id))).ToListAsync();
        }

        public async Task<TObject> SaveAsync(TObject obj)
        {
            await _collection.FindOneAndReplaceAsync(o => o.Id == obj.Id, obj, new FindOneAndReplaceOptions<TObject, ILongDto>
            {
                IsUpsert = true
            });

            return obj;
        }

        async Task<IEnumerable<TObject>> IGenericAsyncRepository<TObject, long>.SaveAsync(IReadOnlyList<TObject> objs)
        {
            var bulks = objs.Select(obj =>
                new ReplaceOneModel<TObject>(Builders<TObject>.Filter.Where(s => s.Id == obj.Id), obj)
                    { IsUpsert = true }).ToList();

            await _collection.BulkWriteAsync(bulks, new BulkWriteOptions
            {
                IsOrdered = true
            });
            return new List<TObject>(objs);
        }


        public async Task DeleteByIdAsync(long id)
        {
            await _collection.DeleteOneAsync(obj => obj.Id == id);
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            await _collection.DeleteManyAsync(o => ids.Any(id => o.Id == id));
        }

        public async Task<IEnumerable<TObject>> GetAllMatchingAsync(Func<TObject, bool> predicate)
        {
            Expression<Func<TObject, bool>> expr = o => predicate(o);
            return await (await _collection.FindAsync(expr)).ToListAsync();
        }

        public async Task<TObject> GetFirstMatchingOrDefaultAsync(Func<TObject, bool> predicate)
        {
            Expression<Func<TObject, bool>> expr = o => predicate(o);
            return await (await _collection.FindAsync(expr)).FirstOrDefaultAsync();
        }
    }
}