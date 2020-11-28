using System;
using System.Linq;
using System.Threading.Tasks;
using MicroBootstrap.Mongo;
using Game.Services.EventProcessor.Core.Entities;
using Game.Services.EventProcessor.Core.Messages.Queries;
using Game.Services.EventProcessor.Core.Repositories;
using MicroBootstrap.Queries;

namespace Game.Services.EventProcessor.Infrastructure.Mongo.Repositories
{
    internal class GameEventSourceMongoRepository : IGameEventSourceRepository
    {
        private readonly IMongoRepository<GameEventSource, Guid> _repository;

        public GameEventSourceMongoRepository(IMongoRepository<GameEventSource, Guid> repository)
            => _repository = repository;

        public Task AddAsync(GameEventSource gameEventSource)
            => _repository.AddAsync(gameEventSource);

        public Task UpdateAsync(GameEventSource gameEventSource)
            => _repository.UpdateAsync(gameEventSource);

        public Task DeleteAsync(GameEventSource gameEventSource)
            => _repository.DeleteAsync(gameEventSource.Id);

        public Task<GameEventSource> GetAsync(Guid id)
            => _repository.GetAsync(d => d.Id == id);

        public Task<bool> ExistsAsync(Guid id)
                 => _repository.ExistsAsync(r => r.Id == id);

        public IQueryable<GameEventSource> GetAll()
            => _repository.GetAll();

        public async Task<PagedResult<GameEventSource>> BrowseAsync(BrowseGameEventSource query)
            => await _repository.BrowseAsync(x => true, query);

    }
}