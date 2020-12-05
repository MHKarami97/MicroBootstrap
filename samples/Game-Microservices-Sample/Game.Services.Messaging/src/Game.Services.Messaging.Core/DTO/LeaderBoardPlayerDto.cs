using System;

namespace Game.Services.Messaging.Core.DTO
{
    public class LeaderBoardPlayerDto
    {
        public Guid UserId { get; set; }
        public int UserRank { get; set; }
        public int UserScore { get; set; }
        public LeaderBoardItemDto LeaderBoardPlayerAhead { get; set; }
        public LeaderBoardItemDto LeaderBoardPlayerBehind { get; set; }
    }
}