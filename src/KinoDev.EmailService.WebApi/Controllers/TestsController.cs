using Microsoft.AspNetCore.Mvc;

namespace KinoDev.EmailService.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        [HttpGet()]
        public IActionResult Get()
        {
            return Ok("Hello from TestsController");
        }
    }
}
