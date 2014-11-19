using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AuditHubCentral.Model;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Rebus;
using Rebus.Shared;

namespace AuditHubCentral.Persistance
{
    public class MessagePersister
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string[] _blackList =
        {
            "DC.Server.BackendService.Messages.SubscriberServiceMessage"
        };

        readonly MongoCollection<TransportMessage> _auditMessages;
        public MessagePersister(MongoCollection<TransportMessage> auditMessages)
        {
            _auditMessages = auditMessages;
        }

        public void HandleAuditMessage(ReceivedTransportMessage message)
        {
            Log.DebugFormat("Audit message: {0}", message.Id);

            Insert(_auditMessages, message);
        }



        void Insert(MongoCollection<TransportMessage> collection, ReceivedTransportMessage message)
        {
            var label = message.Label;
            if (_blackList.Contains(label))
            {
                Log.InfoFormat("Message of type {0} was ignored. Reason: blacklisted", label);
                return;
            }

            try
            {

                var doc = new TransportMessage
                {
                    TransportMessageId = message.Id,
                    From = message.Headers.GetHeaderValueOrDefault(Headers.ReturnAddress),
                    To = message.Headers.GetHeaderValueOrDefault(Headers.AuditSourceQueue),
                    AuditReason = message.Headers.GetHeaderValueOrDefault(Headers.AuditReason),
                    Headers = message.Headers.ToDictionary(d => d.Key, d => d.Value),
                    Body = message.Body,
                    Label = message.Label,
                    DeserializedBody = TryDeserialize(message),
                    CorrelationId = message.Headers.GetHeaderValueOrDefault(Headers.CorrelationId),
                    CopyTime = TryGetCopyTime(message.Headers.GetHeaderValueOrDefault(Headers.AuditMessageCopyTime))
                };

                collection.Save(doc);

                Log.InfoFormat("Message of type {0} saved as {1} => {2}", label, doc.Id, typeof (TransportMessage).Name);
            }
            catch (Exception e)
            {
                Log.WarnFormat("An error occurred while attempting to save message of type {0} in {1}!: {2}", label, typeof (TransportMessage).Name, e);
                throw;
            }
        }

        private DateTime? TryGetCopyTime(string auditMessageCopyTime)
        {
            DateTime result;
            var parsed = DateTime.TryParse(auditMessageCopyTime, out result);
            return parsed ? (DateTime?) result : null;
        }


        private BsonDocument TryDeserialize(ReceivedTransportMessage receivedTransportMessage)
        {
            var headers = receivedTransportMessage.Headers;

            if (!headers.ContainsKey(Headers.Encoding))
            {
                return BsonDocWith("Missing '{0}' header!", Headers.Encoding);
            }

            var encoding = headers[Headers.Encoding] as string;
            if (encoding == null)
            {
                return BsonDocWith("Value of '{0}' header was null!", Headers.Encoding);
            }

            var decoder = Encoding.GetEncoding(encoding);
            var bodyText = decoder.GetString(receivedTransportMessage.Body);

            var bsonDocument = BsonDocument.Parse(bodyText);

            MutatePropertyNames(bsonDocument);

            return bsonDocument;
        }

        private void MutatePropertyNames(BsonDocument bsonDocument)
        {
            var properties = bsonDocument.ToList();

            foreach (var property in properties)
            {
                var bsonValue = property.Value;

                if (property.Name.StartsWith("$"))
                {
                    bsonDocument.Remove(property.Name);
                    var newProperty = new BsonElement(property.Name.Replace("$", "¤"), bsonValue);
                    bsonDocument.Add(newProperty);
                }

                if (bsonValue is BsonDocument)
                {
                    MutatePropertyNames((BsonDocument)bsonValue);
                }

                if (bsonValue is BsonArray)
                {
                    foreach (var containedValue in ((BsonArray)bsonValue))
                    {
                        if (containedValue is BsonDocument)
                        {
                            MutatePropertyNames((BsonDocument)containedValue);
                        }
                    }
                }
            }
        }

        private BsonDocument BsonDocWith(string message, params object[] objs)
        {
            return new BsonDocument { { "message", string.Format(message, objs) } };
        }
    }

    public static class MessagePersisterHelpers
    {
        public static string GetHeaderValueOrDefault(this IDictionary<string, object> headers, string key)
        {
            return headers.ContainsKey(key) ? headers[key].ToString() : null;
        }
    }
}