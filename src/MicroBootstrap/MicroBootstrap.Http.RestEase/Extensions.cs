using System;
using System.Linq;
using System.Net.Http;
using MicroBootstrap.Discovery.Consul.Consul;
using MicroBootstrap.HTTP;
using MicroBootstrap.LoadBalancer.Fabio;
using Microsoft.Extensions.DependencyInjection;
using RestEase;

namespace MicroBootstrap.Http.RestEase
{
    public static class Extensions
    {
        private const string SectionName = "restEase";
        private const string RegistryName = "http.restEase";

        public static IServiceCollection AddRestEaseClient<T>(this IServiceCollection services, string serviceName,
            string sectionName = SectionName, string consulSectionName = "consul", string fabioSectionName = "fabio",
            string httpClientSectionName = "httpClient")
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }
            
            var restEaseOptions = services.GetOptions<RestEaseOptions>(sectionName);
            return services.AddRestEaseClient<T>(serviceName, restEaseOptions,
                b => b.AddFabio(fabioSectionName, consulSectionName, httpClientSectionName));
        }


        public static IServiceCollection AddRestEaseClient<T>(this IServiceCollection builder, string serviceName,
            RestEaseOptions options, ConsulOptions consulOptions, FabioOptions fabioOptions,
            HttpClientOptions httpClientOptions)
            where T : class
            => builder.AddRestEaseClient<T>(serviceName, options,
                b => b.AddFabio(fabioOptions, consulOptions, httpClientOptions));

        private static IServiceCollection AddRestEaseClient<T>(this IServiceCollection services, string serviceName, 
            RestEaseOptions options, Action<IServiceCollection> registerFabio)
            where T : class
        {
            var clientName = typeof(T).ToString();
            
            switch (options.LoadBalancer?.ToLowerInvariant())
            {
                case "consul":
                    services.AddConsulHttpClient(clientName, serviceName);
                    break;
                case "fabio":
                    services.AddFabioHttpClient(clientName, serviceName);
                    break;
                default:
                    ConfigureDefaultClient(services, clientName, serviceName, options);
                    break;
            }

            ConfigureForwarder<T>(services, clientName);

            registerFabio(services);

            return services;
        }

        private static void ConfigureDefaultClient(IServiceCollection services, string clientName,
            string serviceName, RestEaseOptions options)
        {
            services.AddHttpClient(clientName, client =>
            {
                var service = options.Services.SingleOrDefault(s => s.Name.Equals(serviceName,
                    StringComparison.InvariantCultureIgnoreCase));
                if (service is null)
                {
                    throw new RestEaseServiceNotFoundException($"RestEase service: '{serviceName}' was not found.",
                        serviceName);
                }

                client.BaseAddress = new UriBuilder
                {
                    Scheme = service.Scheme,
                    Host = service.Host,
                    Port = service.Port
                }.Uri;
            });
        }

        private static void ConfigureForwarder<T>(IServiceCollection services, string clientName) where T : class
        {
            services.AddTransient<T>(c => new RestClient(c.GetService<IHttpClientFactory>().CreateClient(clientName))
            {
                RequestQueryParamSerializer = new QueryParamSerializer()
            }.For<T>());
        }
    }
}