using System;
using System.Threading;
using System.Threading.Tasks;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Middlewares
{
    internal class ProcessUniqueMessagesMiddleware : StagedMiddleware
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessUniqueMessagesMiddleware> _logger;
        public override string StageMarker { get; } = global::RawRabbit.Pipe.StageMarker.MessageDeserialized;

        public ProcessUniqueMessagesMiddleware(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<ProcessUniqueMessagesMiddleware>>();
        }

        public override async Task InvokeAsync(IPipeContext context,
            CancellationToken token = new CancellationToken())
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var messageProcessor = scope.ServiceProvider.GetRequiredService<IMessageProcessor>();
                var messageId = context.GetDeliveryEventArgs().BasicProperties.MessageId;
                _logger.LogTrace($"Received a unique message with id: {messageId} to be processed.");
                if (!await messageProcessor.TryProcessAsync(messageId))
                {
                    _logger.LogTrace($"A unique message with id: {messageId} was already processed.");
                    return;
                }

                try
                {
                    _logger.LogTrace($"Processing a unique message with id: {messageId}...");
                    await Next.InvokeAsync(context, token);
                    _logger.LogTrace($"Processed a unique message with id: {messageId}.");
                }
                catch
                {
                    _logger.LogTrace($"There was an error when processing a unique message with id: {messageId}.");
                    await messageProcessor.RemoveAsync(messageId);
                    throw;
                }
            }
        }
    }
}