using System;
using System.Collections.Generic;
using System.Linq;
using MicroBootstrap.Discovery.Consul.Consul;
using MicroBootstrap.Discovery.Consul.Models;
using MicroBootstrap.HTTP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroBootstrap.LoadBalancer.Fabio
{
    public static class Extensions
    {
        private const string SectionName = "fabio";

        public static IServiceCollection AddFabio(this IServiceCollection services, string sectionName = SectionName,
            string consulSectionName = "consul", string httpClientSectionName = "httpClient")
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var fabioOptions = services.GetOptions<FabioOptions>(sectionName);
            var consulOptions = services.GetOptions<ConsulOptions>(consulSectionName);
            var httpClientOptions = services.GetOptions<HttpClientOptions>(httpClientSectionName);
            return services.AddFabio(fabioOptions, httpClientOptions, b => b.AddConsul(consulOptions, httpClientOptions));
        }

        public static IServiceCollection AddFabio(this IServiceCollection services, FabioOptions fabioOptions,
            ConsulOptions consulOptions, HttpClientOptions httpClientOptions)
            => services.AddFabio(fabioOptions, httpClientOptions, b => b.AddConsul(consulOptions, httpClientOptions));

        private static IServiceCollection AddFabio(this IServiceCollection services, FabioOptions fabioOptions,
            HttpClientOptions httpClientOptions, Action<IServiceCollection> registerConsul)
        {
            registerConsul(services);
            services.AddSingleton(fabioOptions);

            if (!fabioOptions.Enabled)
            {
                return services;
            }
            //here if we set Type in HttpClientOption in our appsettings to 'fabio' we will resolve a custom HttpClient for our FabioHttpClient that will resolve using IHttpClient 
            //that is a reason to create abstraction on HttpClient - changing 'type' property will switch implementation injected to our `IHttpClient`
            if (httpClientOptions.Type?.ToLowerInvariant() == "fabio")
            {
                //before send a request to particular service we could override original request uri to send request to fabio endpoint
                services.AddTransient<FabioMessageHandler>();
                services.AddHttpClient<IFabioHttpClient, FabioHttpClient>("fabio-http")
                    .AddHttpMessageHandler<FabioMessageHandler>();

                //HttpClient issue: https://github.com/aspnet/AspNetCore/issues/13346
                services.RemoveHttpClient();
                services.AddHttpClient<IHttpClient, FabioHttpClient>("fabio")
                    .AddHttpMessageHandler<FabioMessageHandler>();
            }

            using var serviceProvider = services.BuildServiceProvider();
            var registration = serviceProvider.GetService<ServiceRegistration>();
            var tags = GetFabioTags(registration.Name, fabioOptions.Service);
            if (registration.Tags is null)
            {
                registration.Tags = tags;
            }
            else
            {
                registration.Tags.AddRange(tags);
            }
            //update consul registry with our tags for services and this ServiceRegistration information will use by ConsulHostedService of our consul and when app is started it will register our service to consul registry
            services.UpdateConsulRegistration(registration);

            return services;
        }

        public static void AddFabioHttpClient(this IServiceCollection services, string clientName, string serviceName)
            => services.AddHttpClient<IHttpClient, FabioHttpClient>(clientName)
                .AddHttpMessageHandler(c => new FabioMessageHandler(c.GetService<FabioOptions>(), serviceName));

        private static void UpdateConsulRegistration(this IServiceCollection services,
            ServiceRegistration registration)
        {
            // var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ServiceRegistration));
            // services.Remove(serviceDescriptor);
            // services.AddSingleton(registration);
            
            //update to use builtin replace in .net core 
            services.Replace(ServiceDescriptor.Singleton(registration));
        }

        private static List<string> GetFabioTags(string consulService, string fabioService)
        {
            var service = (string.IsNullOrWhiteSpace(fabioService) ? consulService : fabioService)
                .ToLowerInvariant();

            return new List<string> { $"urlprefix-/{service} strip=/{service}" };
        }
    }
}
