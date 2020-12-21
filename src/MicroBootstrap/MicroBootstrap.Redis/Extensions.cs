using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace MicroBootstrap.Redis
{
    public static class Extensions
    {
        private const string SectionName = "redis";

        public static IServiceCollection AddRedis(this IServiceCollection services, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName)) sectionName = SectionName;

            var options = services.GetOptions<RedisOptions>(sectionName);
            return services.AddRedis(options);
        }
        public static IServiceCollection AddRedis(this IServiceCollection services, RedisOptions options)
        {
            services.TryAddSingleton(options);
            services.TryAddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(options.ConnectionString));
            services.TryAddTransient(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase(options.Database));

            services.AddStackExchangeRedisCache(o =>
            {
                o.Configuration = options.ConnectionString;
                o.InstanceName = options.Instance;
            });

            return services;
        }
    }
}