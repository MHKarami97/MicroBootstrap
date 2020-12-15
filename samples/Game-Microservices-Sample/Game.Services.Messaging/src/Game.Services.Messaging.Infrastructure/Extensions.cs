using MicroBootstrap.Consul;
using MicroBootstrap.Fabio;
using MicroBootstrap.WebApi;
using Microsoft.AspNetCore.Builder;
using MicroBootstrap.Mongo;
using MicroBootstrap.Redis;
using MicroBootstrap.Jaeger;
using System;
using MicroBootstrap;
using Microsoft.Extensions.Hosting;
using Consul;
using MicroBootstrap.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Game.Services.Messaging.Core.Messages.Events;
using Game.Services.Messaging.Core.Entities;
using Game.Services.Messaging.Infrastructure.Mongo.Repositories;
using Game.Services.Messaging.Core.Repositories;
using MicroBootstrap.MessageBrokers.RabbitMq;
using MicroBootstrap.MessageBrokers;

namespace Game.Services.Messaging.Infrastructure
{

    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IGameEventSourceRepository, GameEventSourceMongoRepository>();

            return serviceCollection
                .AddHttpClient()
                .AddConsul()
                .AddFabio()
                .AddRabbitMq()
                .AddMongo()
                .AddRedis()
                .AddOpenTracing()
                .AddJaeger()
                .AddAppMetrics()
                .AddMongoRepository<GameEventSource, Guid>("gameEventSources")
                .AddInitializers(typeof(IMongoDbInitializer));
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            IHostApplicationLifetime applicationLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            IConsulClient client = app.ApplicationServices.GetService<IConsulClient>();
            IStartupInitializer startupInitializer = app.ApplicationServices.GetService<IStartupInitializer>();

            app.UseErrorHandler()
                 .UseJaeger()
                 .UseAppMetrics()
                 .UseRabbitMq().SubscribeEvent<GameEventSourceAdded>();

            var consulServiceId = app.UseConsul();
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                client.Agent.ServiceDeregister(consulServiceId);
            });
            return app;
        }
    }

}