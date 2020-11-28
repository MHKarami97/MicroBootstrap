using System;

namespace Game.Services.EventProcessor.Application.Exceptions
{
       public class GameEventSourceAlreadyExistsException : AppException
    {
        public override string Code { get; } = "game-event-source_already_exists";
        public Guid Id { get; }

        public GameEventSourceAlreadyExistsException(Guid id) : base($"GameEventSource with id: {id} already exists.")
            => Id = id;
    }
}