using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Sample.Functions
{
    public class HealthCheck
    {
        [FunctionName("healthcheck")]
        public Task<IActionResult> HealthCheckAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request)
        {
            return Task.FromResult<IActionResult>(new OkObjectResult("Healthy"));
        }
    }
}
