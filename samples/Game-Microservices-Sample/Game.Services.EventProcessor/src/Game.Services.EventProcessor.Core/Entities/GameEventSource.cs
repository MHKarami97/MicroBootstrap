using System;
using Game.Services.EventProcessor.Core.Entities.Exceptions;
using MicroBootstrap.Types;
namespace Game.Services.EventProcessor.Core.Entities
{
    public class GameEventSource : IIdentifiable<Guid>
    {
        public Guid Id { get; private set; }
        public bool IsWin { get; private set; }
        public int Score { get; private set; }
        public Guid UserId { get; private set; }
        private GameEventSource()
        {
        }

        public GameEventSource(Guid id, bool isWin, int score, Guid userId)
        {
            if (id == Guid.Empty)
            {
                throw new InvalidGameEventSourceIdException();
            }

            if (userId == Guid.Empty)
            {
                throw new CustomException("Invalid_GameEventSource_UserId", "Invalid GameEventSource UserId.");
            }

            if (score < 0)
            {
                throw new InvalidGameEventSourceScoreException();
            }

            Id = id;
            Score = score;
            IsWin = isWin;
            UserId = userId;
        }
        public GameEventSource UpdateScoreAndIsWin(int score, bool isWin)
        {
            this.Score = score;
            this.IsWin = isWin;
            return this;
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