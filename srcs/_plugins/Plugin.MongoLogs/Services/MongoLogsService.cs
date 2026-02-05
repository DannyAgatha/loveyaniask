using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Plugin.MongoLogs.Utils;

namespace Plugin.MongoLogs.Services
{
    public class MongoLogsService
    {
        private readonly IMongoDatabase _database;

        public MongoLogsService(MongoLogsConfiguration mongoLogsConfiguration)
        {
            var client = new MongoClient(mongoLogsConfiguration.ToString());
            _database = client.GetDatabase(mongoLogsConfiguration.DbName);
        }

        public async Task<List<BsonDocument>> GetLogsAsync(string collectionName, long characterId, DateTime minDate, DateTime maxDate, int? limit = null, int? skip = null)
        {
            FilterDefinition<BsonDocument> characterFilter = Builders<BsonDocument>.Filter.Eq("CharacterId", characterId);
            FilterDefinition<BsonDocument> minDateFilter = Builders<BsonDocument>.Filter.Gte("CreatedAt", minDate);
            FilterDefinition<BsonDocument> maxDateFilter = Builders<BsonDocument>.Filter.Lte("CreatedAt", maxDate);
            SortDefinition<BsonDocument> sortByDate = Builders<BsonDocument>.Sort.Descending("CreatedAt");
            var findOptions = new FindOptions<BsonDocument>
            {
                Limit = limit,
                Skip = skip,
                Sort = sortByDate
            };
            IMongoCollection<BsonDocument> collection = _database.GetCollection<BsonDocument>(collectionName);
            return await (await collection.FindAsync(characterFilter & minDateFilter & maxDateFilter, findOptions)).ToListAsync();
        }
    }
}