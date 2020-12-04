using System;
using Game.Services.EventProcessor.Core.Entities;
using Game.Services.EventProcessor.Core.Entities.Exceptions;
using Shouldly;
using Xunit;

namespace Game.Services.EventProcessor.Tests.Unit.Core.Entities
{
    public class CreateGameEventSourceTests
    {
        private GameEventSource Act(Guid id, bool isWin, int score) => GameEventSource.Create(id, isWin, score);

        [Fact]
        public void game_event_source_with_valid_id_and_score_should_be_created()
        {
            // Arrange
            var id = Guid.NewGuid();
            var isWin = true;
            var score = 10;

            // Act
            var gameEventSource = Act(id, isWin, score);

            // Assert
            gameEventSource.ShouldNotBeNull();
            gameEventSource.Id.ShouldBe(id);
            gameEventSource.IsWin.ShouldBe(isWin);
            gameEventSource.Score.ShouldBe(score);
            // gameEventSource.Events.Count().ShouldBe(1);
            // var @event = gameEventSource.Events.Single();
            // @event.ShouldBeOfType<GameEventSourceCreated>();
        }

        [Fact]
        public void game_event_source_with_empty_guid_should_throw_an_exception()
        {
            var id = Guid.Empty;
            var isWin = true;
            var score = 10;

            var exception = Record.Exception(() => Act(id, isWin, score));

            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<InvalidGameEventSourceIdException>();
        }

        [Fact]
        public void game_event_source_with_negative_score_should_throw_an_exception()
        {
            var id = Guid.NewGuid();
            var isWin = true;
            var score = -10;

            var exception = Record.Exception(() => Act(id, isWin, score));

            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<InvalidGameEventSourceScoreException>();
        }
    }
}