using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AuditHubCentral.Core;
using Castle.Windsor;
using Castle.Windsor.Installer;
using DC.WindowsService;
using log4net;
using log4net.Config;

namespace AuditHubCentral
{
    public class Program : WindsorWindowsService
    {
        private static void Main()
        {
            ConfigureLog4Net();
            var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            log.InfoFormat("Running in environment: {0}", ConfigurationManager.AppSettings["environment"]);


            ServiceHost.Run<Program>("DCC AuditHub", new Options()
                .DependsOnMsmq()
                .DependsOnLocalMongoDb());
        }

        private static void ConfigureLog4Net()
        {
            var fileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "log4net.config");
            XmlConfigurator.ConfigureAndWatch(new FileInfo(fileName));
        }

        public override void ConfigureContainer(IWindsorContainer container)
        {
            InstanceVariablesHolder.Initialize();
            container.Install(FromAssembly.This(new WindsorBootstrap()));
        }
    }
}