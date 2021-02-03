using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game.API.Controllers
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping() => Ok();

        [HttpGet("Test")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public string Test()
        {
            return "OK";
        }
    }
}