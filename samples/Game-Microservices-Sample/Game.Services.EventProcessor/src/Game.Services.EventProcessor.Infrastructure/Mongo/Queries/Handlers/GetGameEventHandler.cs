using System.Linq;
using System.Threading.Tasks;
using Game.Services.EventProcessor.Core.DTO;
using Game.Services.EventProcessor.Core.Entities;
using Game.Services.EventProcessor.Core.Messages.Queries;
using MicroBootstrap.Queries;
using MongoDB.Driver;

namespace Game.Services.EventProcessor.Infrastructure.Mongo.Queries.Handlers
{
    internal sealed class GetGameEventHandler : IQueryHandler<GetGameEventSource, GameEventSourceDto>
    {
        private readonly IMongoDatabase _database;

        public GetGameEventHandler(IMongoDatabase database)
        {
             _database = database;
        }

        public async Task<GameEventSourceDto> HandleAsync(GetGameEventSource query)
        {
            var gameEventSource = await _database.GetCollection<GameEventSource>("gameEventSources")
             .Find(d => d.Id == query.Id).SingleOrDefaultAsync();

            return gameEventSource == null ? null : new GameEventSourceDto
            {
                Id = gameEventSource.Id,
                IsWin = gameEventSource.IsWin,
                Score = gameEventSource.Score
            };
        }
    }
}