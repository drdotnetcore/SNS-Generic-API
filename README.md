# SNS-Generic-API
This is an Amazon AWS API written in C# .NET 7.  It's purpose is to be called from any client software without having to implement time and time again the code that makes it work.  That way it can be added to any project that can call an API without knowing at the client how it is implemented.

### Prereqs:
- .NET SDK 7.0 or 6 with LTS support.  Bear in mind though that there are slight differences.
- AWS SDK for .NET (install it via NuGet package manager)
- An AWS account with access to SNS, and IAM credentials.  You must do this yourself, out of scope of this project.
- You need to create an SNS topic and register and confirm an email so that the messages go out.  You can elaborate on this and create an application for people to subscribe to the notifications and even 
create your own topics, etc.

### These are the steps:

#### 1. Create the Web API project:

```shell
dotnet new webapi -n SnsNotificationApi
```

Navigate to the new folder:

```shell
cd SnsNotificationApi
```

#### 2. Install the AWS SDK for .NET:

Install the AWS SDK for .NET by running the following:

```shell
dotnet add package AWSSDK.SimpleNotificationService

```

Install the package AWSSDK.Extensions.NETCore.Setup so that you can use the service builder's extension method to add the SNS service.

```shell
dotnet add package AWSSDK.Extensions.NETCore.Setup
```

#### 3. Make the AWS Credentials available to the application;

We are going to place them in a file called `appsettings.json` (make sure you don't have this file available anywhere where the settings can be grabbed from source control make sure it is added to .gitignore so that it is never exposed.):

```json
{
  "AWS": {
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "Region": "YOUR_REGION"
  }
}
```

Once you go to production, you will utilize a more secure method for managing AWS credentials such as  secret managers.  Do not skimp on security.  What's the point on learning how to do things wrong, right?

#### 4. Implementing the API:

##### Startup.cs
Ensure AWS services are available through dependency injection:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAWSService<Amazon.SimpleNotificationService.IAmazonSimpleNotificationService>();
    services.AddControllers();
}
```

##### NotificationController.cs
Create a controller that uses the `IAmazonSimpleNotificationService` to send a message:

```csharp
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
```

Here, you create an API endpoint `send` under the `NotificationController` which allows for sending a message to an SNS topic when posted to. Ensure that your `appsettings.json` includes the ARN of your SNS topic:

```json
{
  "AWS": {
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "Region": "YOUR_REGION",
    "TopicArn": "YOUR_SNS_TOPIC_ARN"
  }
}
```

### Note:

- Ensure that your AWS credentials have the correct permissions to publish to an SNS topic.
- Ensure your SNS topic is set up to send emails and that the subscription has been confirmed.
- Always use secure practices when dealing with AWS credentials and sensitive data, utilizing IAM roles, and secret managers where possible.
- This example is simplified and does not include input validation, error handling, logging, and other best practices to keep it readable and straightforward.

Finally, start your application, and you should be able to send POST requests to `http://localhost:<port>/notification/send` to trigger email notifications through AWS SNS.
