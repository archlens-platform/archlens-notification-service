using ArchLens.Notification.Application;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ArchLens.Notification.Tests.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplication();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddApplication_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddApplication();

        // Assert
        act.Should().NotThrow();
    }
}
