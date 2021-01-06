using System;
using System.Linq;
using MicroBootstrap.Discovery.Consul.Http;
using MicroBootstrap.Discovery.Consul.MessageHandlers;
using MicroBootstrap.Discovery.Consul.Models;
using MicroBootstrap.Discovery.Consul.Services;
using MicroBootstrap.HTTP;
using MicroBootstrap.WebApi;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.Discovery.Consul.Consul
{
    public static class Extensions
    {
        private const string DefaultInterval = "5s";
        private const string SectionName = "consul";
        private const string RegistryName = "discovery.consul";

        public static IServiceCollection AddConsul(this IServiceCollection services, string sectionName = SectionName,
            string httpClientSectionName = "httpClient")
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }
            
            var consulOptions = services.GetOptions<ConsulOptions>(sectionName);
            var httpClientOptions = services.GetOptions<HttpClientOptions>(httpClientSectionName);
            return services.AddConsul(consulOptions, httpClientOptions);
        }

        public static IServiceCollection AddConsul(this IServiceCollection services, ConsulOptions options,
            HttpClientOptions httpClientOptions)
        {
            services.AddSingleton(options);
            
            //here if we set Type in HttpClientOption in our appsettings to 'consul' we will resolve a custom HttpClient for our ConsulHttpClient that will resolve using IHttpClient 
            //that is a reason to create abstraction on HttpClient - changing 'type' property will switch implementation injected to our `IHttpClient`
            if (httpClientOptions.Type?.ToLowerInvariant() == "consul")
            {
                //https://www.stevejgordon.co.uk/httpclientfactory-aspnetcore-outgoing-request-middleware-pipeline-delegatinghandlers
                services.AddTransient<ConsulServiceDiscoveryMessageHandler>();
                services.AddHttpClient<IConsulHttpClient, ConsulHttpClient>("consul-http")
                    .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();

                //HttpClient issue: https://github.com/aspnet/AspNetCore/issues/13346
                services.RemoveHttpClient();
                //before send a request to particular service we could add some headers or change the payload like middleware, in this middleware before we send a request we ask consul to give us a available instance and we override original request uri
                services.AddHttpClient<IHttpClient, ConsulHttpClient>("consul")
                    .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>(); 
            }

            services.AddTransient<IConsulServicesRegistry, ConsulServicesRegistry>();
            var registration = services.CreateConsulAgentRegistration(options);
            if (registration is null)
            {
                return services;
            }

            services.AddSingleton(registration);

            return services;
        }

        public static void AddConsulHttpClient(this IServiceCollection services, string clientName, string serviceName)
            => services.AddHttpClient<IHttpClient, ConsulHttpClient>(clientName)
                .AddHttpMessageHandler(c => new ConsulServiceDiscoveryMessageHandler(
                    c.GetService<IConsulServicesRegistry>(),
                    c.GetService<ConsulOptions>(), serviceName, true));

        private static ServiceRegistration CreateConsulAgentRegistration(this IServiceCollection services,
            ConsulOptions options)
        {
            var enabled = options.Enabled;
            var consulEnabled = Environment.GetEnvironmentVariable("CONSUL_ENABLED")?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(consulEnabled))
            {
                enabled = consulEnabled == "true" || consulEnabled == "1";
            }

            if (!enabled)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(options.Address))
            {
                throw new ArgumentException("Consul address can not be empty.",
                    nameof(options.PingEndpoint));
            }

            services.AddHttpClient<IConsulService, ConsulService>(c => c.BaseAddress = new Uri(options.Url));

            //a background service along with our app to register our service instance to consul
            if (services.All(x => x.ServiceType != typeof(ConsulHostedService)))
            {
                services.AddHostedService<ConsulHostedService>();
            }

            var serviceId = string.Empty;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                serviceId = serviceProvider.GetRequiredService<IServiceId>().Id;
            }

            var registration = new ServiceRegistration
            {
                Name = options.Service,
                Id = $"{options.Service}:{serviceId}",
                Address = options.Address,
                Port = options.Port,
                Tags = options.Tags,
                Meta = options.Meta,
                EnableTagOverride = options.EnableTagOverride,
                Connect = options.Connect?.Enabled == true ? new Connect() : null
            };

            if (!options.PingEnabled)
            {
                return registration;
            }
            
            var pingEndpoint = string.IsNullOrWhiteSpace(options.PingEndpoint) ? string.Empty :
                options.PingEndpoint.StartsWith("/") ? options.PingEndpoint : $"/{options.PingEndpoint}";
            if (pingEndpoint.EndsWith("/"))
            {
                pingEndpoint = pingEndpoint.Substring(0, pingEndpoint.Length - 1);
            }

            var scheme = options.Address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
                ? string.Empty
                : "http://";
            var check = new ServiceCheck
            {
                Interval = ParseTime(options.PingInterval),
                DeregisterCriticalServiceAfter = ParseTime(options.RemoveAfterInterval),
                Http = $"{scheme}{options.Address}{(options.Port > 0 ? $":{options.Port}" : string.Empty)}" +
                       $"{pingEndpoint}"
            };
            registration.Checks = new[] {check};

            return registration;
        }

        private static string ParseTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DefaultInterval;
            }

            return int.TryParse(value, out var number) ? $"{number}s" : value;
        }
    }
}