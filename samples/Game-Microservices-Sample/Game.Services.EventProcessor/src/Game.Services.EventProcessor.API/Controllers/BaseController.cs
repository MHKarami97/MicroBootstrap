using System.Threading.Tasks;
using MicroBootstrap.Commands;
using MicroBootstrap.Commands.Dispatchers;
using MicroBootstrap.Queries;
using MicroBootstrap.Queries.Dispatchers;
using Microsoft.AspNetCore.Mvc;

namespace Game.Services.EventProcessor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BaseController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public BaseController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        protected async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
            => await _queryDispatcher.QueryAsync<TResult>(query);

        protected async Task SendAsync<TCommand>(TCommand command) where TCommand : class, ICommand
            => await _commandDispatcher.SendAsync(command);

        protected ActionResult<T> Single<T>(T data)
        {
            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        protected ActionResult<PagedResult<T>> Collection<T>(PagedResult<T> pagedResult)
        {
            if (pagedResult == null)
            {
                return NotFound();
            }

            return Ok(pagedResult);
        }
    }
}