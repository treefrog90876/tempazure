using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Sample.Functions
{
    public class HelloWorld
    {
        private readonly IConfiguration configuration;

        public HelloWorld(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [FunctionName("HelloWorld")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest request,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            using var reader = new StreamReader(request.Body);
            var requestBody = await reader.ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = request.Query["name"].ToString() ?? data?.name;

            var responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("HelloWorldSecure")]
        public async Task<IActionResult> RunSecureAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest request,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a secure request.");

            await Task.CompletedTask;

            return new OkObjectResult("Secured");
        }
    }
}
