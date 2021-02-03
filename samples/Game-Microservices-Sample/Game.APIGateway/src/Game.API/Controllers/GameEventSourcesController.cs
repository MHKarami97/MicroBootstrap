using System;
using System.Threading.Tasks;
using MicroBootstrap.Queries;
using MicroBootstrap.WebApi;
using Game.API.DTO;
using Game.API.Messages.Commands;
using Game.API.Messages.Queries;
using Game.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;
using MicroBootstrap.MessageBrokers;
using Game.API.Infrastructure;
using MicroBootstrap.MessageBrokers.RabbitMQ;

namespace Game.API.Controllers
{
    [Route("game-event-sources")]
    public class GameEventSourcesController : BaseController
    {
        private readonly IGameEventProcessorService _gameEventProcessorService;

        public GameEventSourcesController(IBusPublisher busPublisher, ITracer tracer,
         IGameEventProcessorService eventProcessorService, ICorrelationContextBuilder correlationContextBuilder)
         : base(busPublisher, tracer, correlationContextBuilder)
        {
            this._gameEventProcessorService = eventProcessorService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<GameEventSourceDto>>> Get([FromQuery] BrowseGameEventSource query)
            => Collection(await _gameEventProcessorService.BrowseAsync(query));


        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<GameEventSourceDto>> Get(Guid id)
            => Single(await _gameEventProcessorService.GetAsync(id));

        [HttpPost]
        public async Task Post(AddGameEventSource command)
        {
            await SendAsync(command.Bind(c => c.Id, command.Id == default ? Guid.NewGuid() : command.Id));
        }

        // [HttpPut("{id}")]
        // public async Task<IActionResult>  Put(Guid id, UpdateGameEventSource command)
        //     => await SendAsync(command.Bind(c => c.Id, id), 
        //         resourceId: command.Id, resource: "game-event-sources");

        // [HttpDelete("{id}")]
        // public async Task<IActionResult> Delete(Guid id)
        //     => await SendAsync(new DeleteGameEventSource(id));
    }
}