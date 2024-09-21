using Microsoft.AspNetCore.Mvc;

namespace VirtualRadar.Server.ApiControllers
{
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("api/test/input")]
        public IActionResult RepeatInput(string input)
        {
            return Ok(input);
        }
    }
}
