using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using ReceiverService.Filters;
using ReceiverService.Models;
using ReceiverService.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var stringKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
var key = Encoding.ASCII.GetBytes(stringKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure CosmosClient
string endpointUri = Environment.GetEnvironmentVariable("COSMOSDB_END_POINT_URI"); ;
string primaryKey = Environment.GetEnvironmentVariable("COSMOSDB_PRIMARY_KEY"); ;
CosmosClient cosmosClient = new CosmosClient(endpointUri, primaryKey);

// Registre o repositório como serviço
builder.Services.AddSingleton<IRepository<Receiver>>(sp => new CosmosRepository<Receiver>(cosmosClient, "CosmosDb", "ServiceItems"));

// Add services to the container.

builder.Services.AddControllers(config =>
{
    config.Filters.Add(new TokenAuthorizationFilter(stringKey));
    config.Filters.Add<GlobalExceptionFilter>();
    config.Filters.Add<ExecutionTimeActionFilter>();
});

//builder.Services.AddControllers().AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
