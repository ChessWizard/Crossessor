using Crossessor.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.ConfigureInfrastructureServices();

app.UseHttpsRedirection();

app.Run();