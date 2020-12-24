using Game.API.Controllers.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Game.API.Infrastructure
{
    public interface ICorrelationContextBuilder
    {
        CorrelationContext Build(HttpContext context, string correlationId, string spanContext, string name = null, string resourceId = null);
    }
}