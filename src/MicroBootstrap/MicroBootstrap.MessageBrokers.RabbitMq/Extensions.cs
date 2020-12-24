using System;
using MicroBootstrap.MessageBrokers.RabbitMQ.Processors;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Context;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Middlewares;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Plugins;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Processors;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Publishers;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Serialization;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Subscribers;
using MicroBootstrap.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Configuration;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Instantiation;
using RawRabbit.Pipe;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    public static class Extensions
    {
        public static IBusSubscriber UseRabbitMQ(this IApplicationBuilder app)
            => new BusSubscriber(app);

        private const string SectionName = "rabbitMq";
        private const string RegistryName = "messageBrokers.rabbitMq";

        internal static string GetMessageName(this object message)
            => message.GetType().Name.ToSnakeCase().ToLowerInvariant();

        public static IServiceCollection AddRabbitMQ(this IServiceCollection serviceCollection,
            string sectionName = SectionName,
            string redisSectionName = "redis",
            Func<IRabbitMqPluginRegister, IRabbitMqPluginRegister> plugins = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var options = serviceCollection.GetOptions<RabbitMqOptions>(sectionName);
            var redisOptions = serviceCollection.GetOptions<RedisOptions>(redisSectionName);
            return AddRabbitMQ(serviceCollection, options, plugins,
                b => serviceCollection.AddRedis(redisOptions ?? new RedisOptions()));
        }

        public static IServiceCollection AddRabbitMQ(this IServiceCollection serviceCollection,
            RabbitMqOptions options,
            Func<IRabbitMqPluginRegister, IRabbitMqPluginRegister> plugins,
            Action<IServiceCollection> registerRedis)
        {
            serviceCollection.AddSingleton(options);
            serviceCollection.AddSingleton<IContextProvider, ContextProvider>();
            serviceCollection.AddSingleton<RawRabbitConfiguration>(options);
            serviceCollection.AddSingleton<INamingConventions>((_) => new CustomNamingConventions(options));
            serviceCollection.AddSingleton<IConventionsProvider, ConventionsProvider>();
            serviceCollection.AddTransient<IBusPublisher, BusPublisher>();
            if (options.MessageProcessor?.Enabled == true)
            {
                switch (options.MessageProcessor.Type?.ToLowerInvariant())
                {
                    case "redis":
                        registerRedis(serviceCollection);
                        serviceCollection.AddTransient<IMessageProcessor, RedisMessageProcessor>();
                        break;
                    default:
                        serviceCollection.AddTransient<IMessageProcessor, InMemoryMessageProcessor>();
                        break;
                }
            }
            else
            {
                serviceCollection.AddSingleton<IMessageProcessor, EmptyMessageProcessor>();
            }

            serviceCollection.AddSingleton<ICorrelationContextAccessor>(new CorrelationContextAccessor());
            serviceCollection.AddSingleton<IRabbitMQSerializer>(new RabbitMQSerializer());
            serviceCollection.AddSingleton<IMessagePropertiesAccessor>(new MessagePropertiesAccessor());

            ConfigureBus(serviceCollection, plugins);

            return serviceCollection;
        }

        public static IServiceCollection AddExceptionToMessageMapper<T>(this IServiceCollection services)
            where T : class, IExceptionToMessageMapper
        {
            services.AddTransient<IExceptionToMessageMapper, T>();

            return services;
        }

        private static void ConfigureBus(IServiceCollection serviceCollection, Func<IRabbitMqPluginRegister,
            IRabbitMqPluginRegister> plugins = null)
        {
            serviceCollection.AddSingleton<IInstanceFactory>(serviceProvider =>
            {
                var options = serviceProvider.GetService<RabbitMqOptions>();
                var configuration = serviceProvider.GetService<RawRabbitConfiguration>();
                var namingConventions = serviceProvider.GetService<INamingConventions>();
                var register = plugins?.Invoke(new RabbitMqPluginRegister(serviceProvider));

                var factory = RawRabbitFactory.CreateInstanceFactory(new RawRabbitOptions
                {
                    DependencyInjection = ioc =>
                    {
                        register?.Register(ioc);
                        ioc.AddSingleton(options);
                        ioc.AddSingleton(configuration);
                        ioc.AddSingleton<INamingConventions>(namingConventions);
                    },
                    Plugins = clientBuilder =>
                    {
                        register?.Register(clientBuilder);
                        clientBuilder.UseAttributeRouting()
                            .UseRetryLater()
                            .UpdateRetryInfo()
                            //.UseMessageContext(ctx => ctx.GetDeliveryEventArgs()) //we will use context in scope of subscribe method
                            .UseContextForwarding();

                        if (options.MessageProcessor?.Enabled == true)
                        {
                            clientBuilder.ProcessUniqueMessages();
                        }
                    }
                });
                return factory;
            });

            serviceCollection.AddTransient(context => context.GetService<IInstanceFactory>().Create());
        }


        private static IClientBuilder ProcessUniqueMessages(this IClientBuilder clientBuilder)
        {
            clientBuilder.Register(c => c.Use<ProcessUniqueMessagesMiddleware>());

            return clientBuilder;
        }

        private static IClientBuilder UpdateRetryInfo(this IClientBuilder clientBuilder)
        {
            clientBuilder.Register(c => c.Use<RetryStagedMiddleware>());

            return clientBuilder;
        }
    }
}