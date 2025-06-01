using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KinoDev.EmailService.WebApi.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthCheckController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthCheckController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var healthCheckResult = await _healthCheckService.CheckHealthAsync();
            if (healthCheckResult.Status == HealthStatus.Healthy)
            {
                return Ok(healthCheckResult);
            }

            return StatusCode((int)healthCheckResult.Status, healthCheckResult);
        }
    }
}