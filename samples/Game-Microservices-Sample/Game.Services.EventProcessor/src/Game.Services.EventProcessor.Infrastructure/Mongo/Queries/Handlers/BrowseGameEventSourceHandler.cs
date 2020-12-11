using System.Threading.Tasks;
using System.Linq;
using Game.Services.EventProcessor.Core.Messages.Queries;
using Game.Services.EventProcessor.Core.DTO;
using MicroBootstrap.Queries;
using Game.Services.EventProcessor.Core.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MicroBootstrap.Mongo;

namespace Game.Services.EventProcessor.Infrastructure.Mongo.Queries.Handlers
{
    internal sealed class BrowseGameEventSourceHandler : IQueryHandler<BrowseGameEventSource, PagedResult<GameEventSourceDto>>
    {
        private readonly IMongoDatabase _database;

        public BrowseGameEventSourceHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<PagedResult<GameEventSourceDto>> HandleAsync(BrowseGameEventSource query)
        {
            var pagedResult = await _database.GetCollection<GameEventSource>("gameEventSources")
             .AsQueryable().Where(x => true).PaginateAsync(query);

            var result = pagedResult.Items.Select(c => new GameEventSourceDto
            {
                Id = c.Id,
                Score = c.Score,
                IsWin = c.IsWin
            });

            return PagedResult<GameEventSourceDto>.From(pagedResult, result);
        }
    }
}