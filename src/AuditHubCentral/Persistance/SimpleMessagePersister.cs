using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Rebus;
using Rebus.Shared;

namespace AuditHubCentral.Persistance
{
    public class SimpleMessagePersister
    {
        private readonly MongoDatabase _mongoDatabase;

        private readonly string[] blackList =
        {

        };

        private static readonly ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SimpleMessagePersister(MongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public void Insert(ReceivedTransportMessage receivedTransportMessage, string collectionName)
        {
            var correlationId = (string)receivedTransportMessage.Headers[Headers.CorrelationId];

            var message = receivedTransportMessage;
            var messageType = message.GetType();

            if (blackList.Contains(messageType.Name))
            {
                Log.InfoFormat("Message of type {0} was ignored. Reason: blacklisted", messageType);
                return;
            }


            try
            {
                var document = message.ToBsonDocument();

                var id = ObjectId.GenerateNewId();
                document["_id"] = id;
                document["_received"] = DateTime.UtcNow;
                document["_correlationId"] = correlationId;

                _mongoDatabase.GetCollection(collectionName)
                    .Insert(document);

                Log.InfoFormat("Message of type {0} saved as {1} => {2}", messageType, id, collectionName);
            }
            catch (Exception e)
            {
                Log.WarnFormat(
                    "An error occurred while attempting to save message of type {0} in {1}!: {2}",
                    messageType, collectionName, e);
            }
        }
    }
}