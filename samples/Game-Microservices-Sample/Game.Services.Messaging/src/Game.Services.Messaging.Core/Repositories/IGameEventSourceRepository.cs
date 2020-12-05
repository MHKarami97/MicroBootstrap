using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Services.Messaging.Core.Entities;
using Game.Services.Messaging.Core.Messages.Queries;
using MicroBootstrap.Queries;

namespace Game.Services.Messaging.Core.Repositories
{
    public interface IGameEventSourceRepository
    {
        Task<GameEventSource> GetAsync(Guid id);
        IQueryable<GameEventSource> GetAll();
        Task AddAsync(GameEventSource gameEventSource);
        Task UpdateAsync(GameEventSource gameEventSource);
        Task<PagedResult<GameEventSource>> BrowseAsync(BrowseGameEventSource query);
    }
}