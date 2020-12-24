using System.Threading.Tasks;
using Game.Services.EventProcessor.Core.Entities;
using Game.Services.EventProcessor.Core.Messages.Commands;
using Game.Services.EventProcessor.Core.Messages.Events;
using Game.Services.EventProcessor.Core.Repositories;
using Microsoft.Extensions.Logging;
using MicroBootstrap.Commands;
using Game.Services.EventProcessor.Application.Exceptions;
using MicroBootstrap.MessageBrokers;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using System;

namespace Game.Services.EventProcessor.Application.Commands.Handlers
{
    internal sealed class AddGameEventHandler : ICommandHandler<AddGameEventSource>
    {
        private readonly IGameEventSourceRepository _gameSourceRepository;
        private readonly IBusPublisher _busPublisher;
        private readonly ILogger<AddGameEventSource> _logger;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IMessagePropertiesAccessor _messagePropertiesAccessor;

        public AddGameEventHandler(IGameEventSourceRepository gameSourceRepository,
            IBusPublisher busPublisher, ILogger<AddGameEventSource> logger, ICorrelationContextAccessor correlationContextAccessor, IMessagePropertiesAccessor messagePropertiesAccessor)
        {
            _gameSourceRepository = gameSourceRepository;
            _busPublisher = busPublisher;
            _logger = logger;
            _correlationContextAccessor = correlationContextAccessor;
            _messagePropertiesAccessor = messagePropertiesAccessor;
        }

        public async Task HandleAsync(AddGameEventSource command)
        {
            if (await _gameSourceRepository.ExistsAsync(command.Id))
            {
                throw new GameEventSourceAlreadyExistsException(command.Id);
            }

            var gameSource = new GameEventSource(command.Id, command.IsWin, command.Score);
            await _gameSourceRepository.AddAsync(gameSource);

            var messageId = Guid.NewGuid().ToString("N");//this is unique per message type, each message has its own messageId in rabbitmq
            var correlationId = _messagePropertiesAccessor.MessageProperties.CorrelationId;
            var correlationContext = _correlationContextAccessor.CorrelationContext;
            
            var @event =new GameEventSourceAdded(command.Id, command.Score, command.IsWin, command.UserId);
            await _busPublisher.PublishAsync(@event, messageId: messageId, correlationId: correlationId, messageContext: correlationContext);
        }
    }
}