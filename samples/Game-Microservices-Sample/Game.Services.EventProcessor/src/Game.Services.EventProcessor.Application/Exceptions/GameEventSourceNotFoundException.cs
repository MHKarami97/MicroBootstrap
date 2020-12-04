using System;

namespace Game.Services.EventProcessor.Application.Exceptions
{
    public class GameEventSourceNotFoundException : AppException
    {
        public override string Code { get; } = "game-event-source_not_found";
        public Guid Id { get; }

        public GameEventSourceNotFoundException(Guid id) : base($"GameEventSource with id: {id} was not found.")
            => Id = id;
    }
}