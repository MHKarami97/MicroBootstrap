using System.Threading.Tasks;
using Game.Services.Messaging.Core.DTO;
using Game.Services.Messaging.Core.Messages.Events;
using Game.Services.Messaging.Core.Services;

namespace Game.Services.Messaging.Application.Services
{
    public class HubService : IHubService
    {
        private readonly IHubWrapper _hubContextWrapper;

        public HubService(IHubWrapper hubContextWrapper)
        {
            _hubContextWrapper = hubContextWrapper;
        }

        public async Task PublishUserLeaderBoradInfoAsync(LeaderBoardPlayerDto @event)
        {
            await _hubContextWrapper.PublishToAllAsync("user_leader_board_info_added",
                           new
                           {
                               userId = @event.UserId,
                               userRank = @event.UserRank,
                               userScore = @event.UserScore,
                               leaderBoardPlayerBehind = @event.LeaderBoardPlayerBehind,
                               leaderBoardPlayerAhead = @event.LeaderBoardPlayerAhead,
                           }
                       );
        }
    }
}