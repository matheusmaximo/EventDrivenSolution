using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static Amazon.Lambda.SQSEvents.SQSEvent;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Consumer
{
    public class Function
    {
        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed message: {message.Body}");
            context.Logger.LogLine($"MessageAttributes: {JsonConvert.SerializeObject(message.MessageAttributes)}");

            await Task.CompletedTask;
        }
    }
}
