using Game.Services.Messaging.Core.DTO;
using MicroBootstrap.Queries;

namespace Game.Services.Messaging.Core.Messages.Queries
{
    public class BrowseGameEventSource : PagedQueryBase,IQuery<PagedResult<GameEventSourceDto>>
    {
    }
}