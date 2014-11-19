using System.Reflection;
using AuditHubCentral.Persistance;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DC.WindowsService;
using log4net;

namespace AuditHubCentral.Installers
{
    [InstallerPriority(Priorities.BeforeRebus)]
    public class ServiceInstaller : IWindsorInstaller
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<SimpleMessagePersister>().ImplementedBy<SimpleMessagePersister>());
            container.Register(Component.For<MessagePersister>().ImplementedBy<MessagePersister>());
        }
    }
}