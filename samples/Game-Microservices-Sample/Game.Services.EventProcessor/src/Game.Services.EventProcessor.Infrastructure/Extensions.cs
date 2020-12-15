using MicroBootstrap.Consul;
using MicroBootstrap.Fabio;
using MicroBootstrap.WebApi;
using Microsoft.AspNetCore.Builder;
using MicroBootstrap.Mongo;
using MicroBootstrap.Redis;
using MicroBootstrap.Jaeger;
using Game.Services.EventProcessor.Core.Entities;
using System;
using Game.Services.EventProcessor.Core.Messages.Commands;
using Game.Services.EventProcessor.Core.Messages.Events;
using MicroBootstrap;
using Microsoft.Extensions.Hosting;
using Consul;
using MicroBootstrap.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Game.Services.EventProcessor.Infrastructure.Mongo.Repositories;
using Game.Services.EventProcessor.Core.Repositories;
using MicroBootstrap.Queries;
using MicroBootstrap.MessageBrokers.RabbitMq;
using MicroBootstrap.MessageBrokers;

namespace Game.Services.EventProcessor.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IGameEventSourceRepository, GameEventSourceMongoRepository>();

            return serviceCollection
                .AddQueryHandlers()
                .AddInMemoryQueryDispatcher()
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
                 .UseRabbitMq()
                     .SubscribeCommand<AddGameEventSource>();


            var consulServiceId = app.UseConsul();
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                client.Agent.ServiceDeregister(consulServiceId);
            });
            return app;
        }
    }

}