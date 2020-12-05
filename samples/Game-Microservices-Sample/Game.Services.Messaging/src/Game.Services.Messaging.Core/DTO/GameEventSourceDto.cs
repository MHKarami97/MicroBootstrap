using System;

namespace Game.Services.Messaging.Core.DTO
{
    public class GameEventSourceDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool IsWin { get; set; }
        public int Score { get; set; }
    }
}