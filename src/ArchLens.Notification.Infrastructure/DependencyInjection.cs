using ArchLens.Notification.Domain.Interfaces.NotificationInterfaces;
using ArchLens.Notification.Infrastructure.Consumers;
using ArchLens.Notification.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchLens.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalRWithRedis(configuration);
        services.AddMessaging(configuration);
        services.AddScoped<INotificationSender, SignalRNotificationSender>();
        return services;
    }

    private static void AddSignalRWithRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var signalR = services.AddSignalR();

        var redisConnection = configuration.GetRequiredSection("Redis")["ConnectionString"]
            ?? throw new InvalidOperationException("Configuration 'Redis:ConnectionString' is required");

        signalR.AddStackExchangeRedis(redisConnection, options =>
        {
            options.Configuration.ChannelPrefix =
                new StackExchange.Redis.RedisChannel("archlens", StackExchange.Redis.RedisChannel.PatternMode.Literal);
        });
    }

    private static void AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitSection = configuration.GetRequiredSection("RabbitMQ");
        var host = rabbitSection["Host"] ?? throw new InvalidOperationException("Configuration 'RabbitMQ:Host' is required");
        var username = rabbitSection["Username"] ?? throw new InvalidOperationException("Configuration 'RabbitMQ:Username' is required");
        var password = rabbitSection["Password"] ?? throw new InvalidOperationException("Configuration 'RabbitMQ:Password' is required");

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddConsumer<StatusChangedConsumer>();
            bus.AddConsumer<UserAccountDeletedConsumer>();

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
