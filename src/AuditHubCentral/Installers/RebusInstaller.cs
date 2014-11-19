using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Transactions;
using AuditHubCentral.Persistance;
using AuditHubCentral.Recipient;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DC.WindowsService;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Rebus;
using Rebus.Bus;
using Rebus.Castle.Windsor;
using Rebus.Configuration;
using Rebus.Log4Net;
using Rebus.Shared;
using Rebus.Transports.Msmq;
using ILog = log4net.ILog;

namespace AuditHubCentral.Installers
{
    public class Priorities
    {
        public const int Rebus = InstallerPriorityAttribute.DefaultPriority;

        public const int BeforeRebus = Rebus - 1;
        public const int AfterRebus = Rebus + 1;
    }

    [InstallerPriority(Priorities.Rebus)]
    public class RebusInstaller : IWindsorInstaller
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var inputQueue = ConfigurationManager.AppSettings["auditHubQueue"];

            var persister = container.Resolve<MessagePersister>();
            var msmqMessageReceiver = new MsmqMessageReceiver(inputQueue, message => persister.HandleAuditMessage(message));
            msmqMessageReceiver.Start();

        }
    }
}