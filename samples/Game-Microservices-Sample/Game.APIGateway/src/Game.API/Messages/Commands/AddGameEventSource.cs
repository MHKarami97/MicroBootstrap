using System;
using MicroBootstrap.Commands;
using MicroBootstrap.MessageBrokers;
using Newtonsoft.Json;

namespace Game.API.Messages.Commands
{
    [Message("game-event-sources")]
    public class AddGameEventSource : ICommand
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public int Score { get; private set; }
        public bool IsWin { get; private set; }

        [JsonConstructor]
        public AddGameEventSource(Guid id, int score, bool isWin, Guid userId)
        {
            Id = id;
            UserId = userId;
            Score = score;
            IsWin = isWin;
        }
    }
}