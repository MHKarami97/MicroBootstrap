using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MicroBootstrap.Authentication
{
    public class AccessTokenValidatorMiddleware : IMiddleware
    {
        private readonly IAccessTokenService _accessTokenService;

        public AccessTokenValidatorMiddleware(IAccessTokenService accessTokenService)
        {
            _accessTokenService = accessTokenService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var res = await _accessTokenService.IsCurrentActiveToken();
            if (res)
            {
                await next(context);

                return;
            }
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}