using System;
using System.Threading.Tasks;
using MicroBootstrap.Commands;
using MicroBootstrap.Events;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Tag;
using Polly;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Enrichers.MessageContext;

namespace MicroBootstrap.RabbitMq
{
    public class BusSubscriber : IBusSubscriber
    {
        private readonly ILogger _logger;
        private readonly IBusClient _busClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITracer _tracer;
        private readonly int _retries;
        private readonly int _retryInterval;

        private readonly IExceptionToMessageMapper _exceptionToMessageMapper;

        public BusSubscriber(IApplicationBuilder app)
        {
            _logger = app.ApplicationServices.GetService<ILogger<BusSubscriber>>();
            _exceptionToMessageMapper = _serviceProvider.GetService<IExceptionToMessageMapper>() ??
                                      new EmptyExceptionToMessageMapper();
            _serviceProvider = app.ApplicationServices.GetService<IServiceProvider>();
            _busClient = _serviceProvider.GetService<IBusClient>();
            _tracer = _serviceProvider.GetService<ITracer>();
            var options = _serviceProvider.GetService<RabbitMqOptions>();
            _retries = options.Retries >= 0 ? options.Retries : 3;
            _retryInterval = options.RetryInterval > 0 ? options.RetryInterval : 2;
        }

        public IBusSubscriber SubscribeCommand<TCommand>(Func<TCommand, CustomException, IRejectedEvent> onError = null)
            where TCommand : ICommand
        {
            //insted getting message directly like dispatcher get message from bus and find handler fot it
            _busClient.SubscribeAsync<TCommand, CorrelationContext>(async (command, correlationContext) =>
            {
                //service locator
                var commandHandler = _serviceProvider.GetService<ICommandHandler<TCommand>>();

                var accessor = _serviceProvider.GetService<ICorrelationContextAccessor>();
                accessor.CorrelationContext = correlationContext;

                return await TryHandleAsync(command, correlationContext, () => commandHandler.HandleAsync(command), onError);
            });

            return this;
        }

        public IBusSubscriber SubscribeEvent<TEvent>(Func<TEvent, CustomException, IRejectedEvent> onError = null)
            where TEvent : IEvent
        {
            _busClient.SubscribeAsync<TEvent, CorrelationContext>(async (@event, correlationContext) =>
            {
                //  //service locator
                var eventHandler = _serviceProvider.GetService<IEventHandler<TEvent>>();

                return await TryHandleAsync(@event, correlationContext, () => eventHandler.HandleAsync(@event), onError);
            });

            return this;
        }
        public IBusSubscriber Subscribe<TMessage>(Func<IServiceProvider, TMessage, object, Task> handle)
                   where TMessage : class
        {
            _busClient.SubscribeAsync<TMessage, object>(async (message, correlationContext) =>
            {
                try
                {
                    var accessor = _serviceProvider.GetService<ICorrelationContextAccessor>();
                    accessor.CorrelationContext = correlationContext;
                    var exception = await TryHandleAsync(message, correlationContext, handle);
                    if (exception is null)
                    {
                        return new Ack();
                    }

                    throw exception;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }).GetAwaiter().GetResult();

            return this;
        }

        private Task<Exception> TryHandleAsync<TMessage>(TMessage message, object correlationContext,
            Func<IServiceProvider, TMessage, object, Task> handle)
        {
            var currentRetry = 0;
            var messageName = message.GetMessageName();
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, i => TimeSpan.FromSeconds(_retryInterval));

            return retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var retryMessage = currentRetry == 0
                        ? string.Empty
                        : $"Retry: {currentRetry}'.";

                    var preLogMessage = $"Handling a message: '{messageName}'. {retryMessage}";

                    _logger.LogInformation(preLogMessage);

                    await handle(_serviceProvider, message, correlationContext);

                    var postLogMessage = $"Handled a message: '{messageName}'. {retryMessage}";
                    _logger.LogInformation(postLogMessage);

                    return null;
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    _logger.LogError(ex, ex.Message);
                    var rejectedEvent = _exceptionToMessageMapper.Map(ex, message);
                    if (rejectedEvent is null)
                    {
                        throw new Exception($"Unable to handle a message: '{messageName}' " +
                                            $"retry {currentRetry - 1}/{_retries}...", ex);
                    }

                    await _busClient.PublishAsync(rejectedEvent, ctx => ctx.UseMessageContext(correlationContext));
                    _logger.LogWarning($"Published a rejected event: '{rejectedEvent.GetMessageName()}' " +
                                       $"for the message: '{messageName}'.");

                    return new Exception($"Handling a message: '{messageName}' failed and rejected event: " +
                                         $"'{rejectedEvent.GetMessageName()}' was published.", ex);
                }
            });
        }

        // Internal retry for services that subscribe to the multiple events of the same types.
        // It does not interfere with the routing keys and wildcards (see TryHandleWithRequeuingAsync() below).
        private async Task<Acknowledgement> TryHandleAsync<TMessage>(TMessage message,
            ICorrelationContext correlationContext,
            Func<Task> handle, Func<TMessage, CustomException, IRejectedEvent> onError = null)
        {
            var currentRetry = 0;
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, i => TimeSpan.FromSeconds(_retryInterval));

            var messageName = message.GetType().Name;
            //Use Polly for resiliency
            return await retryPolicy.ExecuteAsync<Acknowledgement>(async () =>
            {
                var scope = _tracer
                    .BuildSpan("executing-handler")
                    .AsChildOf(_tracer.ActiveSpan)
                    .StartActive(true);

                using (scope)
                {
                    var span = scope.Span;

                    try
                    {
                        var retryMessage = currentRetry == 0
                            ? string.Empty
                            : $"Retry: {currentRetry}'.";

                        var preLogMessage = $"Handling a message: '{messageName}' " +
                                      $"with correlation id: '{correlationContext.Id}'. {retryMessage}";

                        _logger.LogInformation(preLogMessage);
                        span.Log(preLogMessage);

                        await handle();

                        var postLogMessage = $"Handled a message: '{messageName}' " +
                                             $"with correlation id: '{correlationContext.Id}'. {retryMessage}";
                        _logger.LogInformation(postLogMessage);
                        span.Log(postLogMessage);

                        return new Ack();
                    }
                    catch (Exception exception)
                    {
                        currentRetry++;
                        _logger.LogError(exception, exception.Message);
                        span.Log(exception.Message);
                        span.SetTag(Tags.Error, true);

                        if (exception is CustomException CustomException && onError != null)
                        {
                            var rejectedEvent = onError(message, CustomException);
                            await _busClient.PublishAsync(rejectedEvent, ctx => ctx.UseMessageContext(correlationContext));
                            _logger.LogInformation($"Published a rejected event: '{rejectedEvent.GetType().Name}' " +
                                                   $"for the message: '{messageName}' with correlation id: '{correlationContext.Id}'.");

                            span.SetTag("error-type", "domain");
                            // the reason is that in this case this is “domain exception” and retrying it will not do the
                            //trick. For instance if user’s password is incorrect (because of same domain validation)
                            // retrying the processing will not make it valid. Thus for this specific type of exceptions
                            // we return explicit ACK.
                            return new Ack();
                        }

                        span.SetTag("error-type", "infrastructure");
                        throw new Exception($"Unable to handle a message: '{messageName}' " +
                                            $"with correlation id: '{correlationContext.Id}', " +
                                            $"retry {currentRetry - 1}/{_retries}...");
                    }
                }
            });
        }

        // RabbitMQ retry that will publish a message to the retry queue.
        // Keep in mind that it might get processed by the other services using the same routing key and wildcards.
        private async Task<Acknowledgement> TryHandleWithRequeuingAsync<TMessage>(TMessage message,
            CorrelationContext correlationContext,
            Func<Task> handle, Func<TMessage, CustomException, IRejectedEvent> onError = null)
        {
            var messageName = message.GetType().Name;
            var retryMessage = correlationContext.Retries == 0
                ? string.Empty
                : $"Retry: {correlationContext.Retries}'.";
            _logger.LogInformation($"Handling a message: '{messageName}' " +
                                   $"with correlation id: '{correlationContext.Id}'. {retryMessage}");

            try
            {
                await handle();
                _logger.LogInformation($"Handled a message: '{messageName}' " +
                                       $"with correlation id: '{correlationContext.Id}'. {retryMessage}");

                return new Ack();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                if (exception is CustomException CustomException && onError != null)
                {
                    var rejectedEvent = onError(message, CustomException);
                    await _busClient.PublishAsync(rejectedEvent, ctx => ctx.UseMessageContext(correlationContext));
                    _logger.LogInformation($"Published a rejected event: '{rejectedEvent.GetType().Name}' " +
                                           $"for the message: '{messageName}' with correlation id: '{correlationContext.Id}'.");

                    return new Ack();
                }

                if (correlationContext.Retries >= _retries)
                {
                    await _busClient.PublishAsync(RejectedEvent.For(messageName),
                        ctx => ctx.UseMessageContext(correlationContext));

                    throw new Exception($"Unable to handle a message: '{messageName}' " +
                                        $"with correlation id: '{correlationContext.Id}' " +
                                        $"after {correlationContext.Retries} retries.", exception);
                }

                _logger.LogInformation($"Unable to handle a message: '{messageName}' " +
                                       $"with correlation id: '{correlationContext.Id}', " +
                                       $"retry {correlationContext.Retries}/{_retries}...");

                return Retry.In(TimeSpan.FromSeconds(_retryInterval));
            }
        }

        private class EmptyExceptionToMessageMapper : IExceptionToMessageMapper
        {
            public object Map(Exception exception, object message) => null;
        }

    }
}