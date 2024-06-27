using CommandLineParser.Application.Builders;
using CommandLineParser.Models;
using FluentAssertions;

namespace CommandLineParser.Tests.Application.Builders;

public class OptionBuilderTests
{
    private readonly OptionBuilder _sut = new();

    [Fact]
    public void Build_ReturnsOption_WhenAllRequiredDataIsProvided()
    {
        // Arrange
        var name = "--opt1";
        var description = "description";
        _sut.WithName(name)
            .WithDescription(description);

        // Act
        var option = _sut.Build();

        // Assert
        option.FullName.Should().Be(name);
        option.Description.Should().Be(description);
        option.Required.Should().BeFalse();
        option.ShortName.Should().BeNull();
        option.Argument.Should().BeNull();
    }

    [Fact]
    public void Build_ReturnsRequiredOption_WhenIsRequiredIsCalled()
    {
        // Arrange
        var name = "--opt1";
        var description = "description";
        _sut.WithName(name)
            .WithDescription(description)
            .IsRequired();

        // Act
        var option = _sut.Build();

        // Assert
        option.FullName.Should().Be(name);
        option.Description.Should().Be(description);
        option.Required.Should().BeTrue();
        option.ShortName.Should().BeNull();
        option.Argument.Should().BeNull();
    }

    [Fact]
    public void Build_ReturnsOptionWithShortName_WhenWithShortNameIsCalled()
    {
        // Arrange
        var name = "--opt1";
        var description = "description";
        var shortName = 'a';
        _sut.WithName(name)
            .WithDescription(description)
            .WithShortName(shortName);

        // Act
        var option = _sut.Build();

        // Assert
        option.FullName.Should().Be(name);
        option.Description.Should().Be(description);
        option.ShortName.Should().Be(shortName);
        option.Required.Should().BeFalse();
        option.Argument.Should().BeNull();
    }

    [Fact]
    public void Build_ReturnsOptionWithArgument_WhenArgumentIsSet()
    {
        // Arrange
        var name = "--opt1";
        var description = "description";
        _sut.WithName(name)
            .WithDescription(description)
            .HasArgumentOfType<string>();

        // Act
        var option = _sut.Build();

        // Assert
        option.FullName.Should().Be(name);
        option.Description.Should().Be(description);
        option.Argument.Should().Match<IArgumentDescriptor>(
            a => a.Name == "<opt1>"
              && a.Description == null
              && !a.Repeated);
        option.ShortName.Should().BeNull();
        option.Required.Should().BeFalse();
    }

    [Fact]
    public void Build_ReturnsOptionWithArgumentWithName_WhenArgumentNameIsSet()
    {
        // Arrange
        var name = "--opt1";
        var description = "description";
        var argumentName = "foo";
        _sut.WithName(name)
            .WithDescription(description)
            .HasArgumentOfType<string>()
            .ThatHasName(argumentName);

        // Act
        var option = _sut.Build();

        // Assert
        option.FullName.Should().Be(name);
        option.Description.Should().Be(description);
        option.Argument.Should().Match<IArgumentDescriptor>(
            a => a.Name == argumentName
              && a.Description == null
              && !a.Repeated);
        option.ShortName.Should().BeNull();
        option.Required.Should().BeFalse();
    }

    [Fact]
    public void Build_ReturnsOptionWithRepeatedArgument_WhenArgumentIsSetToBeRepeated()
    {
        // Arrange
        var name = "--opt1";
        var description = "description";
        _sut.WithName(name)
            .WithDescription(description)
            .HasArgumentOfType<string>()
            .ThatIsRepeated();

        // Act
        var option = _sut.Build();

        // Assert
        option.FullName.Should().Be(name);
        option.Description.Should().Be(description);
        option.Argument.Should().Match<IArgumentDescriptor>(
            a => a.Name == "<opt1>"
              && a.Description == null
              && a.Repeated);
        option.ShortName.Should().BeNull();
        option.Required.Should().BeFalse();
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
        var name = "--opt1";
        _sut.WithName(name);
        var action = _sut.Build;

        // Act, Assert
        action.Should().Throw<InvalidOperationException>();
    }
}