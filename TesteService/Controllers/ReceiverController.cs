using AutoMapper;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ReceiverService.Models;
using ReceiverService.Models.Dtos;
using ReceiverService.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReceiverService.Controllers;

[ApiController]
[Route("[controller]")]
public class ReceiverController : ControllerBase
{
    private readonly IRepository<Receiver> _repository;
    private readonly IMapper _mapper;    

    public ReceiverController(IRepository<Receiver> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Receiver>> GetReceiverById(string id)
    {
        var receiver = await _repository.GetByIdAsync(id);
        if (receiver == null)
        {
            return NotFound();
        }
        return Ok(receiver);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Receiver>>> GetAllReceivers()
    {
        var receivers = await _repository.GetAllAsync();
        return Ok(receivers);
    }

    [HttpPost]
    public async Task<ActionResult> AddReceiver([FromBody] Receiver receiver)
    {
        await _repository.AddAsync(receiver);
        await SendMessageToQueue(receiver);
        return CreatedAtAction(nameof(GetReceiverById), new { id = receiver.Id }, receiver);
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto loginDto)
    {
        if (loginDto.Usuario is null or not "root")
            return Unauthorized(new { message = "Credenciais inválidas." });

        // Gerar o token JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, loginDto.Usuario.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Retornar o token para o cliente
        return Ok(new { Token = tokenString });
    }

    [HttpPost("error")]
    public IActionResult ReceiveJSONTestError([FromBody] Receiver receiver)
    {
        throw new Exception("Testando o filtro de exceção");
    }

    private async Task SendMessageToQueue(Receiver receiver)
    {
        string ServiceBusConnectionString = Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CS");
        const string QueueName = "serviceappmessages";

        // Create a ServiceBusClient object using the connection string to the namespace.
        await using var client = new ServiceBusClient(ServiceBusConnectionString);

        // Create a ServiceBusSender object by invoking the CreateSender method on the ServiceBusClient object, and specifying the queue name. 
        ServiceBusSender sender = client.CreateSender(QueueName);

        try
        {
            var receiverMessageDto = _mapper.Map<ReceiverMessageDto>(receiver);

            // Create a new message to send to the queue.
            string messageJson = JsonConvert.SerializeObject(receiverMessageDto);
            var message = new ServiceBusMessage(messageJson);

            // Send the message to the queue.
            await sender.SendMessageAsync(message);

            Console.WriteLine(message);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
