﻿using System.Configuration;
using AuditHubCentral.Model;
using AuditHubCentral.Persistance;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DC.WindowsService;
using MongoDB.Driver;

namespace AuditHubCentral.Installers
{
    [InstallerPriority(Priorities.BeforeRebus)]
    public class MongoInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var connectionString = ConfigurationManager.AppSettings["mongo"];

            var mongoUrl = new MongoUrl(connectionString);
            var database = new MongoClient(mongoUrl).GetServer().GetDatabase(mongoUrl.DatabaseName);



            container.Register(Component.For<MongoDatabase>().Instance(database));
            container.Register(CollectionComponent<TransportMessage>("messageAudit"));

        }

        private static IRegistration CollectionComponent<TMongoEntity>(string collectionName)
        {
            return Component
                .For<MongoCollection<TMongoEntity>>()
                .UsingFactoryMethod(k => k.Resolve<MongoDatabase>().GetCollection<TMongoEntity>(collectionName));
        }
    }
}