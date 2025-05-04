using System.Diagnostics.CodeAnalysis;
using Carter;
using Crossessor.API.Application.Utilities.Helpers;
using Crossessor.API.Domain.Enums;
using Crossessor.API.Infrastructure.Data.Configurations.SettingConfigurations;
using Crossessor.API.Infrastructure.Data.Context;
using Crossessor.API.Infrastructure.Data.Interceptors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;

namespace Crossessor.API.Infrastructure;

public static partial class InfrastructureServiceRegistrations
{
    private static IServiceCollection AddCrossessorDbContext(this IServiceCollection services,
        IConfiguration configuration)
        => services.AddDbContext<CrossessorDbContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditEntitySaveChangeInterceptor>();
            options.UseNpgsql(configuration.GetConnectionString("DatabaseConnection"))
                .AddInterceptors(auditInterceptor);
        });

    private static IServiceCollection AddInterceptors(this IServiceCollection services)
        => services.AddScoped<AuditEntitySaveChangeInterceptor>();

#pragma warning disable SKEXP0070
    private static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
        => services.AddSingleton<Kernel>(service =>
        {
            var aiConfigurations = service.GetRequiredService<IOptions<AiConfigurations>>()
                                          .Value;
            
            return Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(ModelType.GPT_4o.GetModelByType(),
                    aiConfigurations.OpenAi.Key,
                    serviceId: ModelType.GPT_4o.GetModelByType())
                .AddGoogleAIGeminiChatCompletion(ModelType.Gemini_1_5_Flash.GetModelByType(),
                    aiConfigurations.Gemini.Key,
                    serviceId: ModelType.Gemini_1_5_Flash.GetModelByType())
                .Build();
        });

#pragma warning restore SKEXP0070

    private static IServiceCollection AddLibraries(this IServiceCollection services)
        => services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Crossessor API", Version = "v1" });
            }).AddEndpointsApiExplorer()
            .AddCarter()
            .AddMediatR(configuration => { configuration.RegisterServicesFromAssembly(typeof(Program).Assembly); })
            .AddValidatorsFromAssembly(typeof(Program).Assembly);

    private static IServiceCollection AddSettingConfigurations(this IServiceCollection services,
        IConfiguration configuration)
        => services.Configure<AiConfigurations>(configuration.GetSection("AiConfigurations"));

    private static void ConfigureLibraries(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options => { options.DisplayRequestDuration(); });
        }

        app.MapCarter();
    }
}