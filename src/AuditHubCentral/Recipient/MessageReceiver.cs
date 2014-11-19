using System;
using System.Reflection;
using System.Threading;
using System.Transactions;
using log4net;
using Rebus;
using Rebus.Bus;

namespace AuditHubCentral.Recipient
{
    public abstract class MessageReceiver : IDisposable
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Action<ReceivedTransportMessage> _handleReceivedTransportMessage;
        private readonly ManualResetEvent _workerStopped = new ManualResetEvent(false);
        private readonly Thread _worker;
        private volatile bool _keepWorking = true;

        protected MessageReceiver(string path, Action<ReceivedTransportMessage> handleReceivedTransportMessage)
        {
            _handleReceivedTransportMessage = handleReceivedTransportMessage;
            Path = path;
            _worker = new Thread(DoWork) {IsBackground = true};
        }

        public void Start()
        {
            _worker.Start();
        }

        private void DoWork()
        {
            while (_keepWorking)
            {
                try
                {
                    using (var tx = new TransactionScope())
                    {
                        var ambientTransactionContext = new AmbientTransactionContext();
                        var transportMessage = GetNextMessage(ambientTransactionContext);
                        if (transportMessage == null)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        try
                        {
                            _handleReceivedTransportMessage(transportMessage);
                        }
                        catch (Exception exception)
                        {
                            throw new ApplicationException(string.Format("An error occurre while dispatching transport message with ID {0}", transportMessage.Id), exception);
                        }
                        tx.Complete();
                    }
                }
                catch (Exception exception)
                {
                    Log.Warn("Could not handle message: {0}", exception);
                    Thread.Sleep(2000);
                }
            }

            _workerStopped.Set();
        }

        public string Path { get; private set; }

        protected abstract ReceivedTransportMessage GetNextMessage(ITransactionContext context);

        public virtual void Dispose()
        {
            _keepWorking = false;
            _workerStopped.WaitOne();
        }
    }
}