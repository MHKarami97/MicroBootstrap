using System.Threading.Tasks;
using Game.Services.Messaging.Core.DTO;
using Game.Services.Messaging.Core.Messages.Events;

namespace Game.Services.Messaging.Core.Services
{
    public interface IHubService
    {
        Task PublishUserLeaderBoradInfoAsync(LeaderBoardPlayerDto @event);
    }
}