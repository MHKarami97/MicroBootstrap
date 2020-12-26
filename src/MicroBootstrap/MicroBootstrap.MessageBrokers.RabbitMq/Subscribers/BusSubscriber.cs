using System;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Context;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using Polly;
using RabbitMQ.Client.Events;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Enrichers.MessageContext.Subscribe;
using RawRabbit.Pipe;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Subscribers
{
    public class BusSubscriber : IBusSubscriber
    {
        private readonly ILogger _logger;
        private readonly IBusClient _busClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContextProvider _contextProvider;
        private readonly ITracer _tracer;
        private readonly int _retries;
        private readonly int _retryInterval;
        private readonly bool _requeueFailedMessages;
        private readonly IExceptionToMessageMapper _exceptionToMessageMapper;
        private readonly RabbitMqOptions _options;
        private readonly bool _loggerEnabled;
        private readonly IBusPublisher _busPublisher;
        private readonly IConventionsProvider _conventionsProvider;

        public BusSubscriber(IApplicationBuilder app)
        {
            _serviceProvider = app.ApplicationServices.GetService<IServiceProvider>();
            _contextProvider = app.ApplicationServices.GetService<IContextProvider>();
            _logger = _serviceProvider.GetService<ILogger<BusSubscriber>>();
            _exceptionToMessageMapper = _serviceProvider.GetService<IExceptionToMessageMapper>() ??
                                        new EmptyExceptionToMessageMapper();
            _busClient = _serviceProvider.GetService<IBusClient>();
            _tracer = _serviceProvider.GetService<ITracer>();
            _options = _serviceProvider.GetService<RabbitMqOptions>();
            _conventionsProvider = _serviceProvider.GetService<IConventionsProvider>();
            _loggerEnabled = _options.Logger?.Enabled ?? true;
            _retries = _options.Retries >= 0 ? _options.Retries : 3;
            _busPublisher = app.ApplicationServices.GetService<IBusPublisher>();
            _retryInterval = _options.RetryInterval > 0 ? _options.RetryInterval : 2;
            _requeueFailedMessages = _options.RequeueFailedMessages;
        }

        public IBusSubscriber Subscribe<TMessage>(Func<IServiceProvider, TMessage, object, Task> handle)
            where TMessage : class
        {
            _busClient.SubscribeAsync<TMessage, BasicDeliverEventArgs>(async (message, args) =>
                {
                    try
                    {
                        var conventions = _conventionsProvider.Get<TMessage>();
                        var messageId = args.BasicProperties.MessageId;
                        var correlationId = args.BasicProperties.CorrelationId;
                        var timestamp = args.BasicProperties.Timestamp.UnixTime;
                        var info = string.Empty;
                        
                        if (_loggerEnabled)
                        {
                            info = $" [queue: '{conventions.Queue}', routing key: '{conventions.RoutingKey}', " +
                                   $"exchange: '{conventions.Exchange}']";
                            _logger.LogInformation($"Received a message with id: '{messageId}', " +
                                                   $"correlation id: '{correlationId}', timestamp: {timestamp}{info}.");
                        }

                        var correlationContext = BuildCorrelationContext(args);
                        var messageProperties = BuildMessageProperties(args);

                        //https://github.com/pardahlman/RawRabbit#ack-nack-reject-and-retry
                        var acknowledgement = await TryHandleAsync(message, correlationContext, messageProperties, handle);

                        return acknowledgement;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        return new Reject(false);
                        throw;
                    }
                },
                //we can use UseMessageContext in global level in RawRabbitOptions -> Plugins ->  Plugins = clientBuilder => clientBuilder.UseMessageContext(ctx => ctx.GetDeliveryEventArgs())
                ctx => ctx.UseMessageContext(c => c.GetDeliveryEventArgs()));
            return this;
        }

        private Task<Acknowledgement> TryHandleAsync<TMessage>(TMessage message, object correlationContext, IMessageProperties messageProperties,
            Func<IServiceProvider, TMessage, object, Task> handle)
        {
            var currentRetry = 0;
            var messageName = message.GetMessageName();
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, i => TimeSpan.FromSeconds(_retryInterval));

            return retryPolicy.ExecuteAsync<Acknowledgement>(async () =>
            {
                try
                {
                    var retryMessage = string.Empty;
                    if (_loggerEnabled)
                    {

                        retryMessage = currentRetry == 0
                           ? string.Empty
                           : $"Retry: {currentRetry}'.";
                        var preLogMessage = $"Handling a message: '{messageName}' [id: '{messageProperties.MessageId}'] " +
                                $"with correlation id: '{messageProperties.CorrelationId}'. {retryMessage}";

                        _logger.LogInformation(preLogMessage);
                    }

                    await handle(_serviceProvider, message, correlationContext);

                    if (_loggerEnabled)
                    {
                        var postLogMessage = $"Handled a message: '{messageName}' [id: '{messageProperties.MessageId}'] " +
                                               $"with correlation id: '{messageProperties.CorrelationId}'. {retryMessage}";
                        _logger.LogInformation(postLogMessage);
                    }

                    return new Ack();
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    _logger.LogError(ex, ex.Message);
                    var rejectedEvent = _exceptionToMessageMapper.Map(ex, message);
                    if (rejectedEvent is null)
                    {
                        var errorMessage = $"Unable to handle a message: '{messageName}' [id: '{messageProperties.MessageId}'] " +
                                           $"with correlation id: '{messageProperties.CorrelationId}', " +
                                           $"retry {currentRetry - 1}/{_retries}...";

                        if (currentRetry > 1)
                        {
                            _logger.LogError(errorMessage);
                        }

                        if (currentRetry - 1 < _retries)
                        {
                            throw new Exception(errorMessage, ex);
                        }
                        var error = $"Handling a message: '{messageName}' [id: '{messageProperties.MessageId}'] " +
                                                                 $"with correlation id: '{messageProperties.CorrelationId}' failed.";
                        _logger.LogError(error);
                        return new Nack(_requeueFailedMessages);
                    }

                    var rejectedEventName = rejectedEvent.GetType().Name.ToSnakeCase();
                    await _busPublisher.PublishAsync(rejectedEvent, correlationId: messageProperties.CorrelationId,
                        messageContext: correlationContext);
                    if (_loggerEnabled)
                    {
                        _logger.LogWarning($"Published a rejected event: '{rejectedEventName}' " +
                                           $"for the message: '{messageName}' [id: '{messageProperties.MessageId}'] with correlation id: '{messageProperties.CorrelationId}'.");
                    }

                    _logger.LogError($"Handling a message: '{messageName}' [id: '{messageProperties.MessageId}'] " +
                                     $"with correlation id: '{messageProperties.CorrelationId}' failed and rejected event: " +
                                     $"'{rejectedEventName}' was published.", ex);

                    return new Ack();
                }
            });
        }


        private object BuildCorrelationContext(BasicDeliverEventArgs args)
        {
            using var scope = _serviceProvider.CreateScope();

            var correlationContextAccessor = scope.ServiceProvider.GetService<ICorrelationContextAccessor>();
            var correlationContext = _contextProvider.Get(args.BasicProperties.Headers);
            correlationContextAccessor.CorrelationContext = correlationContext;

            return correlationContext;
        }

        private IMessageProperties BuildMessageProperties(BasicDeliverEventArgs args)
        {
            using var scope = _serviceProvider.CreateScope();
            var messagePropertiesAccessor = scope.ServiceProvider.GetService<IMessagePropertiesAccessor>();
            messagePropertiesAccessor.MessageProperties = new MessageProperties
            {
                MessageId = args.BasicProperties.MessageId,
                CorrelationId = args.BasicProperties.CorrelationId,
                Timestamp = args.BasicProperties.Timestamp.UnixTime,
                Headers = args.BasicProperties.Headers
            };
            return messagePropertiesAccessor.MessageProperties;
        }

        private class EmptyExceptionToMessageMapper : IExceptionToMessageMapper
        {
            public object Map(Exception exception, object message) => null;
        }
    }
}