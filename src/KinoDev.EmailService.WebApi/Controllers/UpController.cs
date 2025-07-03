using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KinoDev.EmailService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Up()
    {
        return Ok($"EmailService ::: Up at {DateTime.UtcNow}");
    }
}