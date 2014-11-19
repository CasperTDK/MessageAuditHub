using System;
using System.Configuration;

namespace AuditHubCentral.Core
{
    public static class InstanceVariablesHolder
    {
        public static void Initialize()
        {
            var envionment = ConfigurationManager.AppSettings["environment"];
            switch (envionment)
            {
                case "Release":
                    InstanceVariables.Setup(EnvironmentEnum.Release);
                    break;
                case "Test":
                    InstanceVariables.Setup(EnvironmentEnum.Test);
                    break;
                case "Debug":
                    InstanceVariables.Setup(EnvironmentEnum.Debug);
                    break;
                default:
                    throw new NotImplementedException("Invalid environment!");
            }
        }

        public static void InitializeAsTest(EnvironmentEnum environmentEnum)
        {
            InstanceVariables.Setup(environmentEnum);
        }

        public static class InstanceVariables
        {
            public static EnvironmentEnum Environment
            {
                get { return _environment; }
            }

            private static EnvironmentEnum _environment;

            internal static void Setup(EnvironmentEnum environment)
            {
                _environment = environment;
            }
        }
    }
}