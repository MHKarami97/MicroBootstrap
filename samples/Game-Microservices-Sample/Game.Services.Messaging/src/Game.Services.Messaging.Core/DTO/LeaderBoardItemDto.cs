using System;

namespace Game.Services.Messaging.Core.DTO
{
    public class LeaderBoardItemDto
    {
        public Guid UserId { get; set; }
        public int UserRank { get; set; }
        public int UserScore { get; set; }
    }
}