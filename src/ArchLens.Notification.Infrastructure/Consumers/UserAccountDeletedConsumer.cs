using ArchLens.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ArchLens.Notification.Infrastructure.Consumers;

public sealed class UserAccountDeletedConsumer(
    ILogger<UserAccountDeletedConsumer> logger)
    : IConsumer<UserAccountDeletedEvent>
{
    public Task Consume(ConsumeContext<UserAccountDeletedEvent> context)
    {
        logger.LogInformation(
            "User account deleted: {UserId} at {Timestamp}. Cleaning up active notification contexts.",
            context.Message.UserId,
            context.Message.Timestamp);

        return Task.CompletedTask;
    }
}
