using System.Threading.Tasks;
using System.Linq;
using Game.Services.EventProcessor.Core.Repositories;
using Game.Services.EventProcessor.Core.Messages.Queries;
using Game.Services.EventProcessor.Core.DTO;
using MicroBootstrap.Queries;

namespace Game.Services.EventProcessor.Application.Handlers.Queries
{
    public class BrowseGameEventSourceHandler : IQueryHandler<BrowseGameEventSource, PagedResult<GameEventSourceDto>>
    {
        private readonly IGameEventSourceRepository _gameEventSourceRepository;

        public BrowseGameEventSourceHandler(IGameEventSourceRepository gameEventSourceRepository)
        {
            _gameEventSourceRepository = gameEventSourceRepository;
        }

        public async Task<PagedResult<GameEventSourceDto>> HandleAsync(BrowseGameEventSource query)
        {
            var pagedResult = await _gameEventSourceRepository.BrowseAsync(query);
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