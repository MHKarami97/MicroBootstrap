using System;
using System.Threading.Tasks;
using Game.Services.EventProcessor.Application.Exceptions;
using Game.Services.EventProcessor.Application.Handlers.Commands;
using Game.Services.EventProcessor.Core.Messages.Commands;
using Game.Services.EventProcessor.Core.Repositories;
using NSubstitute;
using Shouldly;
using Xunit;
using Game.Services.EventProcessor.Core.Entities;
using MicroBootstrap.RabbitMq;
using Game.Services.EventProcessor.Core.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Game.Services.EventProcessor.Tests.Unit.Applications.Handlers
{
    public class AddGameEventSourceHandlerTests
    {
        #region Arrange
        private readonly AddGameEventHandler _handler;
        private readonly IGameEventSourceRepository _gameEventSourceRepository;
        private readonly IBusPublisher _busPublisher;
        private readonly ILogger<AddGameEventSource> _logger;

        public AddGameEventSourceHandlerTests()
        {
            _gameEventSourceRepository = Substitute.For<IGameEventSourceRepository>();
            _busPublisher = Substitute.For<IBusPublisher>();
            _logger = Substitute.For<ILogger<AddGameEventSource>>();
            _handler = new AddGameEventHandler(_gameEventSourceRepository, _busPublisher, _logger);
        }
        #endregion

        private Task Act(AddGameEventSource command) => _handler.HandleAsync(command, CorrelationContext.FromId(command.Id));

        [Fact]
        public async Task add_game_event_source_with_existing_id_should_throw_an_exception()
        {
            //Arrange
            var command = new AddGameEventSource(Guid.NewGuid(), 10, true);
            _gameEventSourceRepository.ExistsAsync(command.Id).Returns(true);

            //Act
            var exception = await Record.ExceptionAsync(async () => await Act(command));

            //Assert
            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<GameEventSourceAlreadyExistsException>();
        }

        [Fact]
        public async Task add_game_event_with_valid_id_should_create_a_new_game_event_source()
        {
            //Arrange
            var command = new AddGameEventSource(Guid.NewGuid(), 10, true);
            var gameEventSource = new GameEventSource(command.Id, command.IsWin, command.Score);
            _gameEventSourceRepository.GetAsync(command.Id).Returns(gameEventSource);

            //Act
            await Act(command);
            
            //Assert
            await _gameEventSourceRepository.Received().AddAsync(Arg.Is<GameEventSource>(gameEvent => gameEvent.Id == gameEventSource.Id));
            var gameEvent = new GameEventSourceAdded(command.Id, command.Score, command.IsWin);
            await _busPublisher.Received().PublishAsync(Arg.Is<GameEventSourceAdded>(gameEvent => gameEvent.Id == gameEvent.Id), Arg.Any<ICorrelationContext>());
        }

    }
}