﻿
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Microsoft.Extensions.DependencyInjection.Test
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Microsoft.ApplicationInsights.AspNetCore.Extensions;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.Extensions.Configuration;

#pragma warning disable CS0618 // TelemetryConfiguration.Active is obsolete. We still test with this for backwards compatibility.
    //[CollectionDefinition(nameof(ApplicationInsightsExtensionsTestsBaseClass), DisableParallelization = true)]
    public abstract class ApplicationInsightsExtensionsTestsBaseClass
    {
        internal const string TestInstrumentationKey = "11111111-2222-3333-4444-555555555555";
        internal const string TestConnectionString = "InstrumentationKey=11111111-2222-3333-4444-555555555555;IngestionEndpoint=http://127.0.0.1";
        internal const string InstrumentationKeyFromConfig = "ApplicationInsights:InstrumentationKey";
        internal const string InstrumentationKeyEnvironmentVariable = "APPINSIGHTS_INSTRUMENTATIONKEY";
        internal const string ConnectionStringEnvironmentVariable = "APPLICATIONINSIGHTS_CONNECTION_STRING";
        internal const string TestEndPointEnvironmentVariable = "APPINSIGHTS_ENDPOINTADDRESS";
        internal const string DeveloperModeEnvironmentVariable = "APPINSIGHTS_DEVELOPER_MODE";
        internal const string TestEndPoint = "http://127.0.0.1/v2/track";

        public static ServiceCollection CreateServicesAndAddApplicationinsightsTelemetry(string jsonPath, string channelEndPointAddress, Action<ApplicationInsightsServiceOptions> serviceOptions = null, bool addChannel = true, bool useDefaultConfig = true)
        {
            var services = GetServiceCollectionWithContextAccessor();
            if (addChannel)
            {
                services.AddSingleton<ITelemetryChannel>(new InMemoryChannel());
            }

            IConfigurationRoot config = null;

            if (jsonPath != null)
            {
                var jsonFullPath = Path.Combine(Directory.GetCurrentDirectory(), jsonPath);
                Console.WriteLine("json:" + jsonFullPath);
                Trace.WriteLine("json:" + jsonFullPath);
                try
                {
                    config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(jsonFullPath).Build();
                }
                catch (Exception)
                {
                    throw new Exception("Unable to build with json:" + jsonFullPath);
                }
            }
            else if (channelEndPointAddress != null)
            {
                config = new ConfigurationBuilder().AddApplicationInsightsSettings(endpointAddress: channelEndPointAddress).Build();
            }
            else
            {
                config = new ConfigurationBuilder().Build();
            }

#if NET46
            // In NET46, we don't read from default configuration or bind configuration. 
            services.AddApplicationInsightsTelemetry(config);
#else
            if (useDefaultConfig)
            {
                services.AddSingleton<IConfiguration>(config);
                services.AddApplicationInsightsTelemetry();
            }
            else
            {
                services.AddApplicationInsightsTelemetry(config);
            }
#endif
            if (serviceOptions != null)
            {
                services.Configure(serviceOptions);
            }
            return services;
        }

        public static ServiceCollection GetServiceCollectionWithContextAccessor()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment() { ContentRootPath = Directory.GetCurrentDirectory() });
            services.AddSingleton<DiagnosticListener>(new DiagnosticListener("TestListener"));
            return services;
        }
    }
}
