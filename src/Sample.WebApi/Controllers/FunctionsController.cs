using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sample.Exceptions;

namespace Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FunctionsController : ControllerBase
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public FunctionsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = Guard.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));
            this.configuration = Guard.ThrowIfNull(configuration, nameof(configuration));
        }

        [HttpGet]
        public async Task<string> GetAsync()
        {
            using var httpClient = this.httpClientFactory.CreateClient();

            httpClient.BaseAddress = new Uri($"https://{this.configuration.GetValue<string>("FunctionsAppHostName")}/api/");

            var response = await httpClient.GetAsync(new Uri("HelloWorld", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        [HttpGet("secure")]
        public async Task<string> GetSecureAsync()
        {
            using var httpClient = this.httpClientFactory.CreateClient();

            var code = this.configuration.GetValue<string>("function:helloworldsecure:default");

            httpClient.BaseAddress = new Uri($"https://{this.configuration.GetValue<string>("FunctionsAppHostName")}/api/");

            var response = await httpClient.GetAsync(new Uri($"HelloWorldSecure?code={code}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
