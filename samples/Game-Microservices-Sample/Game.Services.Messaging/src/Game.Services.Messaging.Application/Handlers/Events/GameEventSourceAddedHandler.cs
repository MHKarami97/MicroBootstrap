using System.Threading.Tasks;
using MicroBootstrap.RabbitMq;
using Game.Services.Messaging.Core.Messages.Events;
using Game.Services.Messaging.Core.Services;
using MicroBootstrap.Events;
using Microsoft.Extensions.Logging;
using Game.Services.Messaging.Core.Repositories;
using Game.Services.Messaging.Core.Entities;
using System.Linq;
using Game.Services.Messaging.Core.DTO;

namespace Game.Services.Messaging.Application.Handlers.Events
{
    public class GameEventSourceAddedHandler : IEventHandler<GameEventSourceAdded>
    {
        private readonly IHubService _hubService;
        private readonly IGameEventSourceRepository _gameEventSourceRepository;
        private readonly ILogger<GameEventSourceAddedHandler> _logger;

        public GameEventSourceAddedHandler(IHubService hubService, ILogger<GameEventSourceAddedHandler> logger
         , IGameEventSourceRepository gameEventSourceRepository
         )
        {
            _gameEventSourceRepository = gameEventSourceRepository;
            _hubService = hubService;
            _logger = logger;
        }

        public async Task HandleAsync(GameEventSourceAdded @event, ICorrelationContext context)
        {
            var gameEventSource = await _gameEventSourceRepository.GetAsync(@event.Id);
            if (gameEventSource != null)
            {
                gameEventSource.UpdateScoreAndIsWin(@event.Score, @event.IsWin);
                await _gameEventSourceRepository.UpdateAsync(gameEventSource);
            }
            else
            {
                var gameSource = new GameEventSource(@event.Id, @event.IsWin, @event.Score, @event.UserId);
                await _gameEventSourceRepository.AddAsync(gameSource);
            }
            _logger.LogInformation($"GameEventSource Added with id: '{@event.Id}'.");

            LeaderBoardPlayerDto leaderBorderPlayer = CalculatePositionInLeaderBoard(@event);

            await _hubService.PublishUserLeaderBoradInfoAsync(leaderBorderPlayer);
        }

        private LeaderBoardPlayerDto CalculatePositionInLeaderBoard(GameEventSourceAdded @event)
        {
            var data = _gameEventSourceRepository.GetAll().GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Score = g.Sum(p => p.Score) })
            .OrderByDescending(x => x.Score).ToList();
            var allUsersRank = data.Select((p, i) => new LeaderBoardItemDto
            {
                UserRank = i + 1,
                UserId = p.UserId,
                UserScore = p.Score
            }).ToList();


            var leaderBorderPlayer = new LeaderBoardPlayerDto()
            {
                UserId = @event.UserId,
                UserRank = allUsersRank?.SingleOrDefault(x => x.UserId == @event.UserId).UserRank ?? 0,
                UserScore = allUsersRank?.SingleOrDefault(x => x.UserId == @event.UserId).UserScore ?? 0,
            };
            var userBehind = allUsersRank.SingleOrDefault(x => x.UserRank > leaderBorderPlayer.UserRank && x.UserRank == leaderBorderPlayer.UserRank + 1);
            var userAhead = allUsersRank.SingleOrDefault(x => x.UserRank < leaderBorderPlayer.UserRank && x.UserRank == leaderBorderPlayer.UserRank - 1);

            leaderBorderPlayer.LeaderBoardPlayerAhead = userAhead;
            leaderBorderPlayer.LeaderBoardPlayerBehind = userBehind;
            return leaderBorderPlayer;
        }
    }
}