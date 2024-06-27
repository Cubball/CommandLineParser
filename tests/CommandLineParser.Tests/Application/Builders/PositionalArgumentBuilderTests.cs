using CommandLineParser.Application.Builders;
using FluentAssertions;

namespace CommandLineParser.Tests.Application.Builders;

public class PositionalArgumentBuilderTests
{
    private readonly PositionalArgumentBuilder _sut = new();

    [Fact]
    public void Build_ReturnsPositionalArgument_WhenAllRequiredDataIsProvided()
    {
        // Arrange
        var name = "arg1";
        var description = "description";
        _sut.WithName(name)
            .WithDescription(description)
            .IsOfType<string>();

        // Act
        var positionalArgument = _sut.Build();

        // Assert
        positionalArgument.Name.Should().Be(name);
        positionalArgument.Description.Should().Be(description);
        positionalArgument.Repeated.Should().BeFalse();
    }

    [Fact]
    public void Build_ReturnsRepeatedPositionalArgument_WhenIsRepeatedIsCalled()
    {
        // Arrange
        var name = "arg1";
        var description = "description";
        _sut.WithName(name)
            .WithDescription(description)
            .IsOfType<string>()
            .IsRepeated();

        // Act
        var positionalArgument = _sut.Build();

        // Assert
        positionalArgument.Name.Should().Be(name);
        positionalArgument.Description.Should().Be(description);
        positionalArgument.Repeated.Should().BeTrue();
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenNameIsNotSet()
    {
        // Arrange
        var action = _sut.Build;

        // Act, Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenDescriptionIsNotSet()
    {
        // Arrange
        var name = "arg1";
        _sut.WithName(name);
        var action = _sut.Build;

        // Act, Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenTypeIsNotSet()
    {
        // Arrange
        var name = "arg1";
        var description = "description";
        _sut.WithName(name)
            .WithDescription(description);
        var action = _sut.Build;

        // Act, Assert
        action.Should().Throw<InvalidOperationException>();
    }
}