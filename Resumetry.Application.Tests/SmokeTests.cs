using FluentAssertions;
using Xunit;

namespace Resumetry.Application.Tests;

/// <summary>
/// Smoke tests to verify test infrastructure is working.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void TestInfrastructure_ShouldWork()
    {
        // Arrange
        var expected = 42;

        // Act
        var actual = 42;

        // Assert
        actual.Should().Be(expected);
    }
}
