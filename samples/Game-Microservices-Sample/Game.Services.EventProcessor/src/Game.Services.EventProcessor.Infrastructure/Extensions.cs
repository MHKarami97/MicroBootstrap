using MicroBootstrap.WebApi;
using Microsoft.AspNetCore.Builder;
using MicroBootstrap.Mongo;
using MicroBootstrap.Redis;
using MicroBootstrap.Jaeger;
using Game.Services.EventProcessor.Core.Entities;
using System;
using Game.Services.EventProcessor.Core.Messages.Commands;
using MicroBootstrap;
using Microsoft.Extensions.Hosting;
using Consul;
using MicroBootstrap.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Game.Services.EventProcessor.Infrastructure.Mongo.Repositories;
using Game.Services.EventProcessor.Core.Repositories;
using MicroBootstrap.Queries;
using MicroBootstrap.MessageBrokers;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.Discovery.Consul.Consul;
using MicroBootstrap.LoadBalancer.Fabio;

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
                .AddRabbitMQ()
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
                 .UseRabbitMQ()
                     .SubscribeCommand<AddGameEventSource>();

            return app;
        }
    }

}