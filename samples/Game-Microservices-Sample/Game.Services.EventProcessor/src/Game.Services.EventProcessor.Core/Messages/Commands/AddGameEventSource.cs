using System;
using Newtonsoft.Json;
using MicroBootstrap.Commands;
using MicroBootstrap.Messages;

namespace Game.Services.EventProcessor.Core.Messages.Commands
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
            Score = score;
            IsWin = isWin;
            UserId = userId;
        }
    }
}