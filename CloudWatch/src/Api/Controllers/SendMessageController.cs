using Amazon.CloudWatchEvents;
using Amazon.CloudWatchEvents.Model;
using Amazon.Lambda.Core;
using ApiDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class SendMessageController : ControllerBase
    {
        private readonly IAmazonCloudWatchEvents _client;
        private readonly ILogger<SendMessageController> _logger;

        public SendMessageController(IAmazonCloudWatchEvents client, ILogger<SendMessageController> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post(ApplicationMessage message)
        {
            var request = CreatePublishRequest(message);
            _logger.LogInformation("Request: {request}", JsonConvert.SerializeObject(request));

            var response = await _client.PutEventsAsync(request, default);
            _logger.LogInformation("Response: {response}", JsonConvert.SerializeObject(response));

            var result = CreateResult(response);
            _logger.LogInformation("Result: {result}", JsonConvert.SerializeObject(result));

            return result;
        }

        private IActionResult CreateResult(PutEventsResponse response)
        {
            int httpStatusCode = (int)response.HttpStatusCode;
            if (httpStatusCode < 200 || httpStatusCode > 299)
            {
                _logger.LogError(JsonConvert.SerializeObject(response));
            }
            var result = new StatusCodeResult(httpStatusCode);
            return result;
        }

        private PutEventsRequest CreatePublishRequest(ApplicationMessage message)
        {
            string invokedLambdaArn = GetInvokedLambdaArn();

            var request = new PutEventsRequest
            {
                Entries = new List<PutEventsRequestEntry>
                {
                    new PutEventsRequestEntry
                    {
                        Source = "Genesis.PoC.EventDrivenSolution.CloudWatch",
                        Detail = JsonConvert.SerializeObject(message),
                        DetailType = $"new {nameof(ApplicationMessage)} received",
                        Resources = new List<string>
                        {
                            invokedLambdaArn
                        }
                    }
                }
            };
            return request;
        }

        private string GetInvokedLambdaArn()
        {
            HttpContext.Items.TryGetValue("LambdaContext", out object lambdaContext);
            var invokedLambdaArn = ((ILambdaContext)lambdaContext)?.InvokedFunctionArn;
            return invokedLambdaArn;
        }
    }
}
