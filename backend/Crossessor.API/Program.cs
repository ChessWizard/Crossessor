using Crossessor.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices(builder.Configuration);

const string WEB_CLIENT_CORS_POLICY_NAME = "Crossessor.Client.Web";
builder.Services.AddCors(options =>
{
    options.AddPolicy(WEB_CLIENT_CORS_POLICY_NAME, policy =>
    {
        const string CLIENT_URL = "http://localhost:3000";
        policy
            .WithOrigins(CLIENT_URL)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors(WEB_CLIENT_CORS_POLICY_NAME);

app.ConfigureInfrastructureServices();

app.UseHttpsRedirection();

app.Run();