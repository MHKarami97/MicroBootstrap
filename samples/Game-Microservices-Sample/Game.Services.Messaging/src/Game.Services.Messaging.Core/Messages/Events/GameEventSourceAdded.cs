using System;
using MicroBootstrap.Events;
using MicroBootstrap.MessageBrokers;
using Newtonsoft.Json;

namespace Game.Services.Messaging.Core.Messages.Events
{
    [Message("game-event-sources")]
    public class GameEventSourceAdded : IEvent
    {
        public Guid Id { get; }
        public Guid UserId { get; }
        public int Score { get; }
        public bool IsWin { get; }

        [JsonConstructor]
        public GameEventSourceAdded(Guid id, int score, bool isWin, Guid userId)
        {
            Id = id;
            Score = score;
            IsWin = isWin;
            UserId = userId;
        }
    }
}