using Microsoft.AspNetCore.Mvc;

namespace Game.API.Controllers
{
    [Route("")]
    public class HomeController : ControllerBase
    {   
        [HttpGet("ping")]
        public IActionResult Ping() => Ok();
    }
}