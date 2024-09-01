using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReceiverService.Models.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConsumerService.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsumerController : ControllerBase
{

    [HttpGet("test")]
    public IActionResult GetTest()
    {
        return Ok("Consumer Service Is Working");
    }

    [HttpPost]
    public async void ProcessQueue()
    {        
        ReceiveMessageAsync().GetAwaiter().GetResult();
    }

    static async Task ReceiveMessageAsync()
    {
        string ServiceBusConnectionString = Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CS");
        const string QueueName = "serviceappmessages";

        var client = new ServiceBusClient(ServiceBusConnectionString);

        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        };

        await using ServiceBusProcessor processor = client.CreateProcessor(QueueName, processorOptions);

        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;


        await processor.StartProcessingAsync();
        await processor.CloseAsync();

    }

    // handle received messages
    static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        Console.WriteLine($"Received: {body}");

        // complete the message. messages is deleted from the queue. 
        await args.CompleteMessageAsync(args.Message);
    }

    // handle any errors when receiving messages
    static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}
