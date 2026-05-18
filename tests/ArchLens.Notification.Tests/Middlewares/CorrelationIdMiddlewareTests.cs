using ArchLens.Notification.Api.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace ArchLens.Notification.Tests.Middlewares;

public class CorrelationIdMiddlewareTests
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    [Fact]
    public async Task InvokeAsync_WithExistingCorrelationId_ShouldPreserveIt()
    {
        // Arrange
        var expectedId = "my-correlation-id-123";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = expectedId;

        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.Headers[CorrelationIdHeader].ToString().Should().Be(expectedId);
        context.Items[CorrelationIdHeader].Should().Be(expectedId);
    }

    [Fact]
    public async Task InvokeAsync_WithoutCorrelationId_ShouldGenerateNewOne()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseHeader = context.Response.Headers[CorrelationIdHeader].ToString();
        responseHeader.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(responseHeader, out _).Should().BeTrue();
        context.Items[CorrelationIdHeader].Should().Be(responseHeader);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyCorrelationId_ShouldGenerateNewOne()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = "";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseHeader = context.Response.Headers[CorrelationIdHeader].ToString();
        responseHeader.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(responseHeader, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceCorrelationId_ShouldGenerateNewOne()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = "   ";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseHeader = context.Response.Headers[CorrelationIdHeader].ToString();
        responseHeader.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInResponseHeaders()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = correlationId;
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey(CorrelationIdHeader);
        context.Response.Headers[CorrelationIdHeader].ToString().Should().Be(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInContextItems()
    {
        // Arrange
        var correlationId = "test-id-456";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = correlationId;
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey(CorrelationIdHeader);
        context.Items[CorrelationIdHeader].Should().Be(correlationId);
    }
}
