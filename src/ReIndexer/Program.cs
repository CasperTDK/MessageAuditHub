using System;
using System.Configuration;
using AuditHubCentral.Model;
using MongoDB.Driver;
using Nest;

namespace ReIndexer
{
    class Program
    {
        private static string _indexName;
        private static ElasticClient _elasticClient;

        private static ElasticClient CreateElasticClient()
        {
            var setting = new ConnectionSettings(new Uri("http://localhost:9200"));
            setting.SetDefaultIndex(_indexName);
            return new ElasticClient(setting);
        }

        static void Main(string[] args)
        {
            _indexName = "messageauditstore-" + DateTime.Now.ToString("yyyy-MM-dd");

            //elastic search
            _elasticClient = CreateElasticClient();
            var elasticSearchIndexer = new ElasticSearchIndexer(_elasticClient, _indexName);

            //MongoDB
            var database = GetDatabase();
            var collection = database.GetCollection<TransportMessage>("messageAudit");

            if (!_elasticClient.IndexExists(_indexName).Exists)
            {
                Console.WriteLine("Creating index");
                _elasticClient.CreateIndex(_indexName, c => c
                    .NumberOfReplicas(0)
                    .NumberOfShards(1)
                    .Settings(s => s //just as an example
                        .Add("merge.policy.merge_factor", "10")
                        .Add("search.slowlog.threshold.fetch.warn", "1s")
                    )
                    .AddMapping<TransportMessageWrapper>(m => m.MapFromAttributes()
                    ));
            }
            foreach (var transportMessage in collection.FindAll())
            {
                var newGuid = Guid.NewGuid();
                try
                {
                    var document = new TransportMessageWrapper()
                    {
                        // Message = transportMessage,
                        Id = newGuid,
                        TimeStamp = transportMessage.DownloadTime,
                        DeserializedBody = transportMessage.DeserializedBody.ToString(),
                        CorrelationId = transportMessage.CorrelationId,
                        Label = transportMessage.Label
                    };

                    elasticSearchIndexer.IndexDocument(document);
                }
                catch (Exception)
                {
                }
               
                Console.WriteLine("Indexed " + transportMessage.Id);
            }


            Console.WriteLine("Press any key to quit");
            Console.Read();
        }


        /// <summary>
        /// Elastic search does not support BsonDocuments etc, so we only index and map relevant search fields.
        /// Full document can be found in MongoDB using correlationId
        /// </summary>
        public class TransportMessageWrapper
        {
            [ElasticProperty(Index = FieldIndexOption.NotAnalyzed)]
            public Guid Id { get; set; }
            public DateTime TimeStamp { get; set; }
            public string CorrelationId { get; set; }
            public string DeserializedBody { get; set; }
            public string Label { get; set; }
        }

        private static MongoDatabase GetDatabase()
        {
            var connectionString = ConfigurationManager.AppSettings["mongo"];

            var mongoUrl = new MongoUrl(connectionString);
            var database = new MongoClient(mongoUrl).GetServer().GetDatabase(mongoUrl.DatabaseName);
            return database;
        }
    }

    public class ElasticSearchIndexer
    {
        private readonly ElasticClient _elasticClient;
        private readonly string _index;

        public ElasticSearchIndexer(ElasticClient elasticClient, string index)
        {
            _elasticClient = elasticClient;
            _index = index;
        }

        public TDocument Get<TDocument>(Guid id) where TDocument : class
        {
            return _elasticClient.Get<TDocument>(id.ToString()).Source;
        }

        public void IndexDocument<TDocument>(TDocument document) where TDocument : class
        {
            _elasticClient.Index(document, y => y.Index(_index));
        }

    }
}
