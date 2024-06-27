using CommandLineParser.Application.Builders;
using CommandLineParser.Models;
using FluentAssertions;

namespace CommandLineParser.Tests.Application.Builders;

public class CommandBuilderTests
{
    private readonly CommandBuilder _sut = new();

    [Fact]
    public void Build_ReturnsCommand_WhenOnlyNecessaryInformationIsProvided()
    {
        // Arrange
        var name = "main";
        var description = "description";
        var handler = (ParsedCommand _) => { };
        _sut.WithName(name)
            .WithDescription(description)
            .IsHandledBy(handler);

        // Act
        var command = _sut.Build();

        // Assert
        command.Name.Should().Be(name);
        command.Description.Should().Be(description);
        command.Handler.Should().Be(handler);
        command.Subcommands.Should().BeEmpty();
        command.Arguments.Should().BeEmpty();
        command.Options.Should().BeEmpty();
    }

    [Fact]
    public void Build_ReturnsCommandWithSubcommand_WhenSubcommandWasAdded()
    {
        // Arrange
        var name = "main";
        var description = "description";
        var handler = (ParsedCommand _) => { };
        var subcommandName = "sub";
        var subcommandDescription = "sub description";
        var subcommandHandler = (ParsedCommand _) => { };
        _sut.WithName(name)
            .WithDescription(description)
            .HasSubcommand(subcommand => subcommand
                .WithName(subcommandName)
                .WithDescription(subcommandDescription)
                .IsHandledBy(subcommandHandler))
            .IsHandledBy(handler);

        // Act
        var command = _sut.Build();

        // Assert
        command.Name.Should().Be(name);
        command.Description.Should().Be(description);
        command.Handler.Should().Be(handler);
        command.Subcommands.Should().Satisfy(subcommand =>
            subcommand.Name == subcommandName
            && subcommand.Description == subcommandDescription
            && subcommand.Handler == subcommandHandler);
        command.Arguments.Should().BeEmpty();
        command.Options.Should().BeEmpty();
    }

    [Fact]
    public void Build_ReturnsCommandWithOption_WhenOptionWasAdded()
    {
        // Arrange
        var name = "main";
        var description = "description";
        var handler = (ParsedCommand _) => { };
        var optionName = "--opt1";
        var optionDescription = "opt description";
        _sut.WithName(name)
            .WithDescription(description)
            .HasOption(option => option
                .WithName(optionName)
                .WithDescription(optionDescription))
            .IsHandledBy(handler);

        // Act
        var command = _sut.Build();

        // Assert
        command.Name.Should().Be(name);
        command.Description.Should().Be(description);
        command.Handler.Should().Be(handler);
        command.Subcommands.Should().BeEmpty();
        command.Arguments.Should().BeEmpty();
        command.Options.Should().Satisfy(option =>
            option.FullName == optionName
            && option.Description == optionDescription);
    }

    [Fact]
    public void Build_ReturnsCommandWithPositionalArgument_WhenPositionalArgumentWasAdded()
    {
        // Arrange
        var name = "main";
        var description = "description";
        var handler = (ParsedCommand _) => { };
        var argumentName = "arg1";
        var argumentDescription = "arg description";
        _sut.WithName(name)
            .WithDescription(description)
            .HasPositionalArgument(argument => argument
                .WithName(argumentName)
                .WithDescription(argumentDescription)
                .IsOfType<string>())
            .IsHandledBy(handler);

        // Act
        var command = _sut.Build();

        // Assert
        command.Name.Should().Be(name);
        command.Description.Should().Be(description);
        command.Handler.Should().Be(handler);
        command.Subcommands.Should().BeEmpty();
        command.Arguments.Should().Satisfy(argument =>
                argument.Name == argumentName
                && argument.Description == argumentDescription);
        command.Options.Should().BeEmpty();
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
        var name = "main";
        _sut.WithName(name);
        var action = _sut.Build;

        // Act, Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenHandlerIsNotSet()
    {
        // Arrange
        var name = "main";
        var description = "description";
        _sut.WithName(name)
            .WithDescription(description);
        var action = _sut.Build;

        // Act, Assert
        action.Should().Throw<InvalidOperationException>();
    }
}