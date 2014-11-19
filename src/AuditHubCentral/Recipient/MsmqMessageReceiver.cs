using System;
using Rebus;
using Rebus.Transports.Msmq;

namespace AuditHubCentral.Recipient
{
    public class MsmqMessageReceiver : MessageReceiver
    {
        private readonly MsmqMessageQueue _messageQueue;

        public MsmqMessageReceiver(string path, Action<ReceivedTransportMessage> handleReceivedTransportMessage)
            : base(path, handleReceivedTransportMessage)
        {
            _messageQueue = new MsmqMessageQueue(path);
        }

        public override void Dispose()
        {
            base.Dispose();

            _messageQueue.Dispose();
        }

        protected override ReceivedTransportMessage GetNextMessage(ITransactionContext context)
        {
            return _messageQueue.ReceiveMessage(context);
        }
    }
}
