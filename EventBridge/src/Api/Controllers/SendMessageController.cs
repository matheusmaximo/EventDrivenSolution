using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Api.Infrastructure.Config;
using ApiDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class SendMessageController : ControllerBase
    {
        private readonly IAmazonSimpleNotificationService _client;
        private readonly ApiConfig _config;
        private readonly ILogger<SendMessageController> _logger;

        public SendMessageController(IAmazonSimpleNotificationService client, ApiConfig config, ILogger<SendMessageController> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post(ApplicationMessage message)
        {
            var request = CreatePublishRequest(message);

            var response = await _client.PublishAsync(request, default);

            var result = CreateResult(response);

            return result;
        }

        private IActionResult CreateResult(PublishResponse response)
        {
            int httpStatusCode = (int)response.HttpStatusCode;
            if (httpStatusCode < 200 || httpStatusCode > 299)
            {
                _logger.LogError(JsonConvert.SerializeObject(response));
            }
            var result = new StatusCodeResult(httpStatusCode);
            return result;
        }

        private PublishRequest CreatePublishRequest(ApplicationMessage message)
        {
            var request = new PublishRequest
            {
                Message = message.Body,
                TopicArn = _config.EventTopicArn
            };
            request.MessageAttributes.Add(nameof(message.ConsumerTypes), new MessageAttributeValue { DataType = "String.Array", StringValue = JsonConvert.SerializeObject(message.ConsumerTypes) });
            return request;
        }
    }
}
