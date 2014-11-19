using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace AuditHubCentral.Model
{
    public class TransportMessage
    {
        public TransportMessage()
        {
            Id = ObjectId.GenerateNewId().ToString();
            DownloadTime = DateTime.UtcNow;
        }

        public string From { get; set; }
        public string To { get; set; }
        public string AuditReason { get; set; }

        public DateTime DownloadTime { get; set; }
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public string TransportMessageId { get; set; }
        public Dictionary<string, object> Headers { get; set; }
        public byte[] Body { get; set; }
        public BsonDocument DeserializedBody { get; set; }
        public string Label { get; set; }


        public string GetBodyTextOrNull()
        {
            if (!Headers.ContainsKey(Rebus.Shared.Headers.Encoding)) return null;

            var encodingId = Headers[Rebus.Shared.Headers.Encoding] as string;
            if (encodingId == null) return null;

            try
            {
                var encoding = Encoding.GetEncoding(encodingId);
                return encoding.GetString(Body);
            }
            catch
            {
                return null;
            }
        }
    }
}