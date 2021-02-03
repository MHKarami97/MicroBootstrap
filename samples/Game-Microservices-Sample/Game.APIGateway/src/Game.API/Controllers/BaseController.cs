using System;
using System.Linq;
using System.Threading.Tasks;
using MicroBootstrap.Commands;
using MicroBootstrap.MessageBrokers;
using MicroBootstrap.Queries;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using Game.API.Infrastructure;
using Game.API.Controllers.Infrastructure;
using MicroBootstrap;
using Newtonsoft.Json.Linq;

namespace Game.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private static readonly string AcceptLanguageHeader = "accept-language";
        private static readonly string OperationHeader = "X-Operation";
        private static readonly string ResourceHeader = "X-Resource";
        private static readonly string DefaultCulture = "en-us";
        private static readonly string PageLink = "page";
        private readonly IBusPublisher _busPublisher;
        private readonly ITracer _tracer;
        private readonly ICorrelationContextBuilder _correlationContextBuilder;

        //inject ITracer for OpenTracer and jaeger
        public BaseController(
          IBusPublisher busPublisher
        , ITracer tracer
        , ICorrelationContextBuilder correlationContextBuilder
        )
        {
            _busPublisher = busPublisher;
            _tracer = tracer;
            _correlationContextBuilder = correlationContextBuilder;
        }

        protected ActionResult Single<T>(T model, Func<T, bool> criteria = null)
        {
            if (model == null)
            {
                return NotFound();
            }
            var isValid = criteria == null || criteria(model);
            if (isValid)
            {
                return Ok(model);
            }

            return NotFound();
        }

        protected ActionResult<T> Result<T>(T model, Func<T, bool> criteria = null)
        {
            if (model == null)
            {
                return NotFound();
            }
            var isValid = criteria == null || criteria(model);
            if (isValid)
            {
                return Ok(model);
            }

            return NotFound();
        }

        protected ActionResult Collection<T>(PagedResult<T> pagedResult, Func<PagedResult<T>, bool> criteria = null)
        {
            if (pagedResult == null)
            {
                return NotFound();
            }
            var isValid = criteria == null || criteria(pagedResult);
            if (!isValid)
            {
                return NotFound();
            }
            if (pagedResult.IsEmpty)
            {
                return Ok(Enumerable.Empty<T>());
            }
            Response.Headers.Add("Link", GetLinkHeader(pagedResult));
            Response.Headers.Add("X-Total-Count", pagedResult.TotalResults.ToString());

            return Ok(pagedResult.Items);
        }

        protected async Task SendAsync<T>(T message) where T : class
        {
            var spanContext = _tracer.ActiveSpan is null ? string.Empty : _tracer.ActiveSpan.Context.ToString();
            var messageId = Guid.NewGuid().ToString("N");//this is unique per message type, each message has its own messageId in rabbitmq
            var correlationId = Guid.NewGuid().ToString("N");//unique for whole message flow , here gateway initiate our correlationId along side our newly publish message to keep track of our request

            var resourceId = Guid.NewGuid().ToString("N");
            if (HttpContext.Request.Method == "POST" && message is JObject jObject)
            {
                jObject.SetResourceId(resourceId);
            }
            var correlationContext = _correlationContextBuilder.Build(HttpContext, correlationId, spanContext, message.GetType().Name.ToSnakeCase(), resourceId);
            await _busPublisher.PublishAsync<T>(message, messageId: messageId, correlationId: correlationId, spanContext: spanContext, messageContext: correlationContext);
            HttpContext.Response.StatusCode = 202; //we send 202 status code and a correlationId immediately to end user after we published message to the message broker 
            HttpContext.Response.SetOperationHeader(correlationId);
        }

        protected ActionResult Accepted(ICorrelationContext context)
        {
            Response.Headers.Add(OperationHeader, $"operations/{context.ConnectionId}");
            if (!string.IsNullOrWhiteSpace(context.Resource))
            {
                Response.Headers.Add(ResourceHeader, context.Resource);
            }

            return base.Accepted();
        }

        protected bool IsAdmin
            => User.IsInRole("admin");

        protected Guid UserId
            => string.IsNullOrWhiteSpace(User?.Identity?.Name) ?
                Guid.Empty :
                Guid.Parse(User.Identity.Name);

        protected string Culture
            => Request.Headers.ContainsKey(AcceptLanguageHeader) ?
                    Request.Headers[AcceptLanguageHeader].First().ToLowerInvariant() :
                    DefaultCulture;

        private string GetLinkHeader(PagedResultBase result)
        {
            var first = GetPageLink(result.CurrentPage, 1);
            var last = GetPageLink(result.CurrentPage, result.TotalPages);
            var prev = string.Empty;
            var next = string.Empty;
            if (result.CurrentPage > 1 && result.CurrentPage <= result.TotalPages)
            {
                prev = GetPageLink(result.CurrentPage, result.CurrentPage - 1);
            }
            if (result.CurrentPage < result.TotalPages)
            {
                next = GetPageLink(result.CurrentPage, result.CurrentPage + 1);
            }

            return $"{FormatLink(next, "next")}{FormatLink(last, "last")}" +
                   $"{FormatLink(first, "first")}{FormatLink(prev, "prev")}";
        }

        private string GetPageLink(int currentPage, int page)
        {
            var path = Request.Path.HasValue ? Request.Path.ToString() : string.Empty;
            var queryString = Request.QueryString.HasValue ? Request.QueryString.ToString() : string.Empty;
            var conjunction = string.IsNullOrWhiteSpace(queryString) ? "?" : "&";
            var fullPath = $"{path}{queryString}";
            var pageArg = $"{PageLink}={page}";
            var link = fullPath.Contains($"{PageLink}=")
                ? fullPath.Replace($"{PageLink}={currentPage}", pageArg)
                : fullPath += $"{conjunction}{pageArg}";

            return link;
        }

        private static string FormatLink(string path, string rel)
            => string.IsNullOrWhiteSpace(path) ? string.Empty : $"<{path}>; rel=\"{rel}\",";
    }
}