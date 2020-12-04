using System;
using System.Threading.Tasks;
using MicroBootstrap.Commands.Dispatchers;
using MicroBootstrap.Queries;
using MicroBootstrap.Queries.Dispatchers;
using MicroBootstrap.WebApi;
using Game.Services.EventProcessor.Core.DTO;
using Game.Services.EventProcessor.Core.Messages.Commands;
using Game.Services.EventProcessor.Core.Messages.Queries;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Game.Services.EventProcessor.API.Controllers
{
    [Route("game-event-sources")]
    public class GameEventSourcesController : BaseController
    {
        public GameEventSourcesController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher) :
            base(commandDispatcher, queryDispatcher)
        {
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<GameEventSourceDto>>> Get([FromQuery] BrowseGameEventSource query)
            => Collection(await QueryAsync(query));

        [HttpGet("{id}")]
        public async Task<ActionResult<GameEventSourceDto>> Get([FromRoute] GetGameEventSource query)
            => Single(await QueryAsync(query));

        [HttpPost]
        public async Task<ActionResult> Post(AddGameEventSource command)
        {
            await SendAsync(command.Bind(c => c.Id, command.Id == Guid.Empty ? Guid.NewGuid() : command.Id));
            return Created($"game-event-sources/{command.Id}", command);
        }
    }
}