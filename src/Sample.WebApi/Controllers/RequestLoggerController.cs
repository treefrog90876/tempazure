using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Sample.Observability;
using Sample.Storage;

namespace Sample.Controllers
{
    [ApiController]
    [Route("requests")]
    public class RequestLoggerController : ControllerBase
    {
        private readonly IStorage storage;
        private readonly ICoreTelemetry telemetry;

        public RequestLoggerController(IStorage storage, ICoreTelemetry telemetry)
        {
            this.storage = storage;
            this.telemetry = telemetry;
        }

        [HttpGet("keys")]
        public IAsyncEnumerable<string> GetIdentifiersAsync()
        {
            using var span = this.telemetry.Start($"{nameof(RequestLoggerController)}-{nameof(GetIdentifiersAsync)}");

            return this.storage.GetIdentifiersAsync();
        }

        [HttpGet]
        public async Task<ActionResult<Stream>> GetAsync([FromQueryAttribute]string key)
        {
            using var span = this.telemetry.Start($"{nameof(RequestLoggerController)}-{nameof(GetAsync)}");

            if (string.IsNullOrWhiteSpace(key))
            {
                return this.BadRequest();
            }

            try
            {
                span.SetTag(nameof(key), key);
                return await this.storage.GetAsync(key);
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound();
            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAsync([FromQuery] string key)
        {
            using var span = this.telemetry.Start($"{nameof(RequestLoggerController)}-{nameof(DeleteAsync)}");

            if (string.IsNullOrWhiteSpace(key))
            {
                return this.BadRequest();
            }

            span.SetTag(nameof(key), key);
            await this.storage.RemoveAsync(key);

            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromQuery] string key)
        {
            using var span = this.telemetry.Start($"{nameof(RequestLoggerController)}-{nameof(PostAsync)}");

            if (string.IsNullOrWhiteSpace(key))
            {
                return this.BadRequest();
            }

            span.SetTag(nameof(key), key);
            await this.storage.CreateAsync(key, this.HttpContext.Request.Body);

            return this.Ok();
        }
    }
}
