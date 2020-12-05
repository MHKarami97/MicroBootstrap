using System;
using MicroBootstrap.Types;
namespace Game.Services.Messaging.Core.Entities
{
    public class GameEventSource : IIdentifiable
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public bool IsWin { get; private set; }
        public int Score { get; private set; }

        private GameEventSource()
        {
        }
        public GameEventSource UpdateScoreAndIsWin(int score, bool isWin)
        {
            this.Score = score;
            this.IsWin = isWin;
            return this;
        }
        public GameEventSource(Guid id, bool isWin, int score, Guid userId)
        {
            if (id == Guid.Empty)
            {
                throw new CustomException("Invalid_GameEventSource_Id", "Invalid GameEventSource Id.");
            }

            if (userId == Guid.Empty)
            {
                throw new CustomException("Invalid_GameEventSource_UserId", "Invalid GameEventSource UserId.");
            }

            if (score < 0)
            {
                throw new CustomException("Invalid_Score",
                    $"Invalid Score: {score}, The score can't be negative.");
            }

            Id = id;
            Score = score;
            IsWin = isWin;
            UserId = userId;
        }

    }
}