using System;
using Game.Services.EventProcessor.Core.Entities.Exceptions;
using MicroBootstrap.Types;
namespace Game.Services.EventProcessor.Core.Entities
{
    public class GameEventSource : IIdentifiable<Guid>
    {
        public Guid Id { get; set; }
        public bool IsWin { get; set; }
        public int Score { get; set; }

        private GameEventSource()
        {
        }

        public GameEventSource(Guid id, bool isWin, int score)
        {
            if (id == Guid.Empty)
            {
                throw new InvalidGameEventSourceIdException();
            }

            if (score < 0)
            {
                throw new InvalidGameEventSourceScoreException();
            }

            Id = id;
            Score = score;
            IsWin = isWin;
        }

        // for new resource we use static factory method and once we fetch data from database and we want to restore that as a aggregate 
        // we will use aggregate constructor and we don't have events in constructor 
        public static GameEventSource Create(Guid id, bool isWin, int score)
        {
            var gameEventSource = new GameEventSource(id, isWin, score); //calling our internal constructor
            //we can add events to our domain model for event sourcing here
            //gameEventSource.AddEvent(new GameEventSourceCreated(gameEventSource));
            return gameEventSource;
        }



    }
}