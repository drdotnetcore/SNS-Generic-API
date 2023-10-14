using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SnsNotificationApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IAmazonSimpleNotificationService _sns;
        private readonly string _topicArn;

        public NotificationController(
            IAmazonSimpleNotificationService sns, 
            IConfiguration configuration)
        {
            _sns = sns;
            _topicArn = configuration["AWS:TopicArn"];
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] string message)
        {
            var request = new PublishRequest
            {
                TopicArn = _topicArn,
                Message = message
            };

            var response = await _sns.PublishAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                return Ok();
            else
                return StatusCode((int)response.HttpStatusCode, response);
        }
    }
}